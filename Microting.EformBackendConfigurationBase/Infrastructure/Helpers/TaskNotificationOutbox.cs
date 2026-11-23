/*
The MIT License (MIT)

Copyright (c) 2007 - 2026 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Microting.EformBackendConfigurationBase.Infrastructure.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Const;
using Data;
using Data.Entities;
using Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

/// <summary>One reminder the planner wants (all times UTC).</summary>
public sealed record TaskNotificationPlan(
    int SdkSiteId,
    int PropertyId,
    int AreaRulePlanningId,
    string PlanDayKey,
    int? ComplianceId,
    string Kind,
    DateTime PlannedStartUtc,
    DateTime SendAtUtc,
    string Title,
    string Body);

/// <summary>Outcome of <see cref="TaskNotificationOutbox.PlanAsync"/>. If RetryIds is non-empty,
/// keep the property dirty and plan it again.</summary>
public sealed record TaskNotificationPlanResult(int Inserted, int Revived, IReadOnlyList<long> RetryIds);

/// <summary>Outcome of <see cref="TaskNotificationOutbox.CancelOrWithdrawAsync"/>. If RetryIds is
/// non-empty, keep the property dirty and plan it again.</summary>
public sealed record TaskNotificationRetractResult(int Cancelled, int WithdrawRequested, IReadOnlyList<long> RetryIds);

/// <summary>
/// The task reminder outbox (<see cref="TaskNotification"/>), shared by the planner (web host) and
/// the deliverer (background service). Every state change goes through this class, as one
/// conditional set-based UPDATE, so neither process can break the state machine or race the other.
/// All times are UTC; a <see cref="DateTimeKind.Local"/> argument is refused, and rows read back
/// carry <see cref="DateTimeKind.Utc"/> (a converter on the entity's columns).
/// <para><b>State machine</b> (numbers in <see cref="TaskNotificationState"/>; never renumber):</para>
/// <code>
/// from              to                by         method
/// (new)             Pending           planner    PlanAsync (duplicate key: ignored)
/// Pending           Cancelled         planner    CancelOrWithdrawAsync
/// Sent              WithdrawPending   planner    CancelOrWithdrawAsync (due at once)
/// Cancelled/Withdrawn Pending         planner    PlanAsync again for the same key (revived, new text)
/// WithdrawPending   Sent              planner    PlanAsync again for the same key (withdraw dropped)
/// Pending           Sending           deliverer  ClaimDueAsync (lease, Attempts + 1)
/// WithdrawPending   Withdrawing       deliverer  ClaimDueAsync (lease, Attempts + 1)
/// Sending           Pending           deliverer  MarkRetryAsync, or ReapExpiredLeasesAsync
/// Withdrawing       WithdrawPending   deliverer  MarkRetryAsync, or ReapExpiredLeasesAsync
/// Sending           Sent              deliverer  MarkSentAsync
/// Sending           Expired           deliverer  MarkExpiredAsync (never send after the planned start)
/// Withdrawing       Withdrawn         deliverer  MarkWithdrawnAsync
/// Sending/Withdrawing Failed          deliverer  MarkFailedAsync
/// </code>
/// <para>
/// <b>Planner contract.</b> In-flight rows (Sending, Withdrawing) belong to the deliverer that
/// claimed them; the planner never changes them, and a deliverer may finish a step at any moment.
/// <see cref="PlanAsync"/> and <see cref="CancelOrWithdrawAsync"/> therefore end with a check, after
/// all their updates, that every row they were asked about is in the wanted end state, and return
/// the ids that are not in <c>RetryIds</c>. <b>If RetryIds is non-empty, keep the property dirty and
/// plan it again</b> (a few seconds later). The check cannot be outdated by a deliverer: from a
/// wanted end state the deliverer only moves a row to another wanted end state.
/// </para>
/// <para>
/// <b>Claims and fencing.</b> A claim is a conditional <c>UPDATE ... SET Attempts = @seen + 1 WHERE
/// Id = @id AND State = @expected AND Attempts = @seen AND NextAttemptAtUtc &lt;= @now</c>, where
/// @seen is the attempt number this call read; InnoDB lets only one of two concurrent updates match,
/// so the affected-row count proves ownership. The claim's token is therefore known to this call
/// (state and @seen + 1; attempt numbers are unique per row); a row whose read-back no longer
/// carries that token (it was reaped and claimed by another deliverer in between) is not returned.
/// Attempts is never reset; every Mark* applies only while the row still has the state AND attempt
/// number of the claim passed in, so a deliverer whose lease expired cannot overwrite a newer claim.
/// </para>
/// </summary>
public static class TaskNotificationOutbox
{
    private static readonly HashSet<string> KnownKinds = [TaskNotificationKinds.TaskDue];

    // ------------------------------------------------------------------ planner side

    /// <summary>
    /// Inserts the reminders that do not exist yet (key: site, task, day, planned start, kind) as
    /// Pending, due at their SendAtUtc. An existing key is left alone, except a retracted row that is
    /// wanted again: Cancelled/Withdrawn becomes Pending (with the new text, if its start is still
    /// ahead) and WithdrawPending goes back to Sent. Idempotent; a duplicate is never an error.
    /// A planned key whose row is, after this call, not in a wanted state (Pending, Sending, Sent;
    /// Expired and Failed are final) is returned in <see cref="TaskNotificationPlanResult.RetryIds"/>,
    /// e.g. a withdraw in flight or one that finished meanwhile (see the class doc).
    /// </summary>
    /// <exception cref="ArgumentException">An invalid row (ids &gt; 0, ComplianceId &gt; 0 when set,
    /// a known kind, a real yyyy-MM-dd day, Title 1-200 and Body 1-400 characters, SendAtUtc not
    /// after PlannedStartUtc, no local times).</exception>
    public static async Task<TaskNotificationPlanResult> PlanAsync(
        BackendConfigurationPnDbContext dbContext,
        IReadOnlyCollection<TaskNotificationPlan> rows,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(rows);
        RequireUtc(nowUtc, nameof(nowUtc));
        foreach (var row in rows)
        {
            Validate(row);
        }

        int inserted = 0, revived = 0;
        var retry = new List<long>();
        foreach (var r in rows)
        {
            // INSERT IGNORE only skips the duplicate key here: every value was validated above, so no
            // other error can be turned into a warning. All values are parameters.
            var added = await dbContext.Database.ExecuteSqlAsync($@"
INSERT IGNORE INTO `TaskNotifications`
    (`SdkSiteId`, `PropertyId`, `AreaRulePlanningId`, `PlanDayKey`, `ComplianceId`, `Kind`,
     `PlannedStartUtc`, `SendAtUtc`, `Title`, `Body`, `State`, `LeaseUntilUtc`, `Attempts`,
     `NextAttemptAtUtc`, `SentAtUtc`, `WithdrawnAtUtc`, `LastError`, `CreatedAtUtc`, `UpdatedAtUtc`)
VALUES
    ({r.SdkSiteId}, {r.PropertyId}, {r.AreaRulePlanningId}, {r.PlanDayKey}, {r.ComplianceId}, {r.Kind},
     {r.PlannedStartUtc}, {r.SendAtUtc}, {r.Title}, {r.Body}, {(byte)TaskNotificationState.Pending}, NULL, 0,
     {r.SendAtUtc}, NULL, NULL, NULL, {nowUtc}, {nowUtc})", cancellationToken).ConfigureAwait(false);
            if (added > 0)
            {
                inserted++;
                continue;
            }

            var sameKey = dbContext.TaskNotifications.Where(n =>
                n.SdkSiteId == r.SdkSiteId && n.AreaRulePlanningId == r.AreaRulePlanningId &&
                n.PlanDayKey == r.PlanDayKey && n.PlannedStartUtc == r.PlannedStartUtc && n.Kind == r.Kind);

            revived += await sameKey
                .Where(n => (n.State == TaskNotificationState.Cancelled || n.State == TaskNotificationState.Withdrawn)
                            && n.PlannedStartUtc > nowUtc)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.State, TaskNotificationState.Pending)
                    .SetProperty(n => n.Title, r.Title)
                    .SetProperty(n => n.Body, r.Body)
                    .SetProperty(n => n.PropertyId, r.PropertyId)
                    .SetProperty(n => n.ComplianceId, r.ComplianceId)
                    .SetProperty(n => n.SendAtUtc, r.SendAtUtc)
                    .SetProperty(n => n.NextAttemptAtUtc, r.SendAtUtc)
                    .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
                    .SetProperty(n => n.SentAtUtc, (DateTime?)null)
                    .SetProperty(n => n.WithdrawnAtUtc, (DateTime?)null)
                    .SetProperty(n => n.LastError, (string)null)
                    .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
                .ConfigureAwait(false);

            revived += await sameKey
                .Where(n => n.State == TaskNotificationState.WithdrawPending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.State, TaskNotificationState.Sent)
                    .SetProperty(n => n.LastError, (string)null)
                    .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
                .ConfigureAwait(false);

            // Wanted end states: Pending, Sending, Sent (Expired, Failed final); others retry (class doc).
            retry.AddRange(await sameKey
                .Where(n => n.PlannedStartUtc > nowUtc
                            && n.State != TaskNotificationState.Pending && n.State != TaskNotificationState.Sending
                            && n.State != TaskNotificationState.Sent && n.State != TaskNotificationState.Expired
                            && n.State != TaskNotificationState.Failed)
                .Select(n => n.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false));
        }

        return new TaskNotificationPlanResult(inserted, revived, retry);
    }

    /// <summary>
    /// Retracts reminders the planner no longer wants: Pending becomes Cancelled (nothing was sent),
    /// Sent becomes WithdrawPending (due at once). A row being sent is not touched; it, and any row
    /// a deliverer moved back to Pending or on to Sent while this call ran, is returned in
    /// <see cref="TaskNotificationRetractResult.RetryIds"/> (see the class doc). Finished rows are
    /// left as they are.
    /// </summary>
    public static async Task<TaskNotificationRetractResult> CancelOrWithdrawAsync(
        BackendConfigurationPnDbContext dbContext,
        IReadOnlyCollection<long> ids,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(ids);
        RequireUtc(nowUtc, nameof(nowUtc));
        foreach (var id in ids)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(ids));
        }

        if (ids.Count == 0)
        {
            return new TaskNotificationRetractResult(0, 0, []);
        }

        var list = ids.Distinct().ToList();
        var cancelled = await dbContext.TaskNotifications
            .Where(n => list.Contains(n.Id) && n.State == TaskNotificationState.Pending)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.State, TaskNotificationState.Cancelled)
                .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
            .ConfigureAwait(false);

        var withdraw = await dbContext.TaskNotifications
            .Where(n => list.Contains(n.Id) && n.State == TaskNotificationState.Sent)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.State, TaskNotificationState.WithdrawPending)
                .SetProperty(n => n.NextAttemptAtUtc, nowUtc)
                .SetProperty(n => n.LastError, (string)null)
                .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
            .ConfigureAwait(false);

        // Unwanted rows must end Cancelled, Withdraw*, Expired or Failed; others retry (class doc).
        var retry = await dbContext.TaskNotifications
            .Where(n => list.Contains(n.Id)
                        && (n.State == TaskNotificationState.Pending || n.State == TaskNotificationState.Sending
                            || n.State == TaskNotificationState.Sent))
            .OrderBy(n => n.Id)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new TaskNotificationRetractResult(cancelled, withdraw, retry);
    }

    /// <summary>Re-renders the text of a Pending row (e.g. the task was renamed). Returns false when
    /// the row is no longer Pending; a sent reminder keeps the text it was sent with.</summary>
    public static async Task<bool> UpdateTextAsync(
        BackendConfigurationPnDbContext dbContext,
        long id,
        string title,
        string body,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        RequireText(title, 200, nameof(title));
        RequireText(body, 400, nameof(body));
        RequireUtc(nowUtc, nameof(nowUtc));

        return await dbContext.TaskNotifications
            .Where(n => n.Id == id && n.State == TaskNotificationState.Pending)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Title, title)
                .SetProperty(n => n.Body, body)
                .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
            .ConfigureAwait(false) == 1;
    }

    /// <summary>All rows of a property for one day, any state, untracked: the planner's diff input.</summary>
    public static Task<List<TaskNotification>> ListForPropertyDayAsync(
        BackendConfigurationPnDbContext dbContext,
        int propertyId,
        string planDayKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(propertyId);
        RequireDayKey(planDayKey, nameof(planDayKey));

        return dbContext.TaskNotifications.AsNoTracking()
            .Where(n => n.PropertyId == propertyId && n.PlanDayKey == planDayKey)
            .OrderBy(n => n.Id)
            .ToListAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ deliverer side

    /// <summary>
    /// Claims up to <paramref name="max"/> due rows (Pending or WithdrawPending with
    /// NextAttemptAtUtc &lt;= now), oldest due first, and returns them untracked in their claimed
    /// state (Sending or Withdrawing) with the lease and attempt number; pass each to exactly one
    /// Mark* call. A row another deliverer claimed first is skipped.
    /// </summary>
    public static async Task<List<TaskNotification>> ClaimDueAsync(
        BackendConfigurationPnDbContext dbContext,
        DateTime nowUtc,
        int max,
        TimeSpan leaseFor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        RequireUtc(nowUtc, nameof(nowUtc));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(leaseFor, TimeSpan.Zero);

        var candidates = await dbContext.TaskNotifications.AsNoTracking()
            .Where(n => (n.State == TaskNotificationState.Pending || n.State == TaskNotificationState.WithdrawPending)
                        && n.NextAttemptAtUtc <= nowUtc)
            .OrderBy(n => n.NextAttemptAtUtc).ThenBy(n => n.Id)
            .Take(max)
            .Select(n => new { n.Id, n.State, n.Attempts })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var leaseUntil = nowUtc + leaseFor;
        // The claim token of each row this call won: state and attempt number.
        var claimed = new List<(long Id, TaskNotificationState State, int Attempts)>();
        foreach (var candidate in candidates)
        {
            var expected = candidate.State;
            var seen = candidate.Attempts;
            var mine = seen + 1;
            var target = expected == TaskNotificationState.Pending
                ? TaskNotificationState.Sending
                : TaskNotificationState.Withdrawing;
            var won = await dbContext.TaskNotifications
                .Where(n => n.Id == candidate.Id && n.State == expected && n.Attempts == seen
                            && n.NextAttemptAtUtc <= nowUtc)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.State, target)
                    .SetProperty(n => n.LeaseUntilUtc, leaseUntil)
                    .SetProperty(n => n.Attempts, mine)
                    .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
                .ConfigureAwait(false);
            if (won == 1)
            {
                claimed.Add((candidate.Id, target, mine));
            }
        }

        if (claimed.Count == 0)
        {
            return [];
        }

        var ids = claimed.Select(c => c.Id).ToList();
        var rows = await dbContext.TaskNotifications.AsNoTracking()
            .Where(n => ids.Contains(n.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Return a row only while it still carries this call's token; otherwise it was reaped and
        // claimed by another deliverer between the UPDATE and this read, and is not ours any more.
        var result = new List<TaskNotification>();
        foreach (var (id, state, attempts) in claimed)
        {
            var row = rows.SingleOrDefault(n => n.Id == id);
            if (row != null && row.State == state && row.Attempts == attempts)
            {
                result.Add(row);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns rows whose lease ended before <paramref name="nowUtc"/> (the deliverer died or hung)
    /// to their queue: Sending to Pending, Withdrawing to WithdrawPending. They are due again at once;
    /// the attempt still counts. Returns the number of rows reaped.
    /// </summary>
    public static async Task<int> ReapExpiredLeasesAsync(
        BackendConfigurationPnDbContext dbContext,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        RequireUtc(nowUtc, nameof(nowUtc));

        var sends = await dbContext.TaskNotifications
            .Where(n => n.State == TaskNotificationState.Sending && n.LeaseUntilUtc < nowUtc)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.State, TaskNotificationState.Pending)
                .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
                .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
            .ConfigureAwait(false);
        var withdraws = await dbContext.TaskNotifications
            .Where(n => n.State == TaskNotificationState.Withdrawing && n.LeaseUntilUtc < nowUtc)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.State, TaskNotificationState.WithdrawPending)
                .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
                .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken)
            .ConfigureAwait(false);
        return sends + withdraws;
    }

    /// <summary>Sending → Sent. False when the claim is stale (see the class doc).</summary>
    public static Task<bool> MarkSentAsync(
        BackendConfigurationPnDbContext dbContext,
        TaskNotification claimed,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
        => MarkAsync(dbContext, claimed, nowUtc, [TaskNotificationState.Sending], s => s
            .SetProperty(n => n.State, TaskNotificationState.Sent)
            .SetProperty(n => n.SentAtUtc, nowUtc)
            .SetProperty(n => n.LastError, (string)null)
            .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
            .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken);

    /// <summary>Withdrawing → Withdrawn. False when the claim is stale.</summary>
    public static Task<bool> MarkWithdrawnAsync(
        BackendConfigurationPnDbContext dbContext,
        TaskNotification claimed,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
        => MarkAsync(dbContext, claimed, nowUtc, [TaskNotificationState.Withdrawing], s => s
            .SetProperty(n => n.State, TaskNotificationState.Withdrawn)
            .SetProperty(n => n.WithdrawnAtUtc, nowUtc)
            .SetProperty(n => n.LastError, (string)null)
            .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
            .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken);

    /// <summary>
    /// A transient failure: back to the row's queue (Sending → Pending, Withdrawing →
    /// WithdrawPending), not claimed again before <paramref name="nextAttemptAtUtc"/>. The caller
    /// picks the back-off (e.g. from <see cref="TaskNotification.Attempts"/>). False when stale.
    /// </summary>
    public static Task<bool> MarkRetryAsync(
        BackendConfigurationPnDbContext dbContext,
        TaskNotification claimed,
        DateTime nextAttemptAtUtc,
        string errorCode,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        RequireUtc(nextAttemptAtUtc, nameof(nextAttemptAtUtc));
        RequireErrorCode(errorCode);
        ArgumentNullException.ThrowIfNull(claimed);
        var queue = claimed.State == TaskNotificationState.Withdrawing
            ? TaskNotificationState.WithdrawPending
            : TaskNotificationState.Pending;
        return MarkAsync(dbContext, claimed, nowUtc, [TaskNotificationState.Sending, TaskNotificationState.Withdrawing], s => s
            .SetProperty(n => n.State, queue)
            .SetProperty(n => n.NextAttemptAtUtc, nextAttemptAtUtc)
            .SetProperty(n => n.LastError, errorCode)
            .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
            .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken);
    }

    /// <summary>Sending → Expired: the planned start has passed, and a reminder is never sent late.
    /// False when stale.</summary>
    public static Task<bool> MarkExpiredAsync(
        BackendConfigurationPnDbContext dbContext,
        TaskNotification claimed,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
        => MarkAsync(dbContext, claimed, nowUtc, [TaskNotificationState.Sending], s => s
            .SetProperty(n => n.State, TaskNotificationState.Expired)
            .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
            .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken);

    /// <summary>Sending or Withdrawing → Failed: a permanent error, or a withdraw that could not be
    /// delivered in time. False when stale.</summary>
    public static Task<bool> MarkFailedAsync(
        BackendConfigurationPnDbContext dbContext,
        TaskNotification claimed,
        string errorCode,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        RequireErrorCode(errorCode);
        return MarkAsync(dbContext, claimed, nowUtc, [TaskNotificationState.Sending, TaskNotificationState.Withdrawing], s => s
            .SetProperty(n => n.State, TaskNotificationState.Failed)
            .SetProperty(n => n.LastError, errorCode)
            .SetProperty(n => n.LeaseUntilUtc, (DateTime?)null)
            .SetProperty(n => n.UpdatedAtUtc, nowUtc), cancellationToken);
    }

    // ------------------------------------------------------------------ retention

    /// <summary>
    /// Physically deletes up to <paramref name="maxRows"/> of the oldest finished rows (Sent, Withdrawn,
    /// Cancelled, Expired, Failed) created before <paramref name="cutoffUtc"/> (the caller passes
    /// now − 30 days) and returns how many it deleted; live rows are never deleted;
    /// repeat while the result equals <paramref name="maxRows"/>.
    /// <para>
    /// Why a hard delete: everything else in this package is soft-deleted (WorkflowState = Removed).
    /// A reminder is worthless once its day is over, and a soft-deleted outbox would grow forever. The
    /// product owner approved deleting rows of this table older than 30 days, an exception to the
    /// soft-delete rule for this table and the change-feed table only.
    /// </para>
    /// </summary>
    public static Task<int> DeleteOlderThanAsync(
        BackendConfigurationPnDbContext dbContext,
        DateTime cutoffUtc,
        int maxRows = 10000,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRows);
        RequireUtc(cutoffUtc, nameof(cutoffUtc));

        // Finished rows only: a live row is never deleted, whatever its age.
        return dbContext.TaskNotifications
            .Where(n => n.CreatedAtUtc < cutoffUtc
                        && (n.State == TaskNotificationState.Sent || n.State == TaskNotificationState.Withdrawn
                            || n.State == TaskNotificationState.Cancelled || n.State == TaskNotificationState.Expired
                            || n.State == TaskNotificationState.Failed))
            .OrderBy(n => n.Id)
            .Take(maxRows)
            .ExecuteDeleteAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ internals

    // The fenced update behind every Mark*: same row, still in the claimed state, still carrying the
    // claim's attempt number.
    private static async Task<bool> MarkAsync(
        BackendConfigurationPnDbContext dbContext,
        TaskNotification claimed,
        DateTime nowUtc,
        TaskNotificationState[] allowedFrom,
        Action<UpdateSettersBuilder<TaskNotification>> setters,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(claimed);
        RequireUtc(nowUtc, nameof(nowUtc));
        if (!allowedFrom.Contains(claimed.State))
        {
            return false;
        }

        var id = claimed.Id;
        var state = claimed.State;
        var attempts = claimed.Attempts;
        return await dbContext.TaskNotifications
            .Where(n => n.Id == id && n.State == state && n.Attempts == attempts)
            .ExecuteUpdateAsync(setters, cancellationToken)
            .ConfigureAwait(false) == 1;
    }

    private static void Validate(TaskNotificationPlan row)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row.SdkSiteId, nameof(row.SdkSiteId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row.PropertyId, nameof(row.PropertyId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row.AreaRulePlanningId, nameof(row.AreaRulePlanningId));
        if (row.ComplianceId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(row.ComplianceId), "A compliance id must be positive when set.");
        }

        RequireDayKey(row.PlanDayKey, nameof(row.PlanDayKey));
        if (row.Kind == null || !KnownKinds.Contains(row.Kind))
        {
            throw new ArgumentException($"Unknown notification kind '{row.Kind}'; use TaskNotificationKinds.", nameof(row.Kind));
        }

        RequireUtc(row.PlannedStartUtc, nameof(row.PlannedStartUtc));
        RequireUtc(row.SendAtUtc, nameof(row.SendAtUtc));
        if (row.SendAtUtc > row.PlannedStartUtc)
        {
            throw new ArgumentException("A reminder cannot be due after the task starts.", nameof(row.SendAtUtc));
        }

        RequireText(row.Title, 200, nameof(row.Title));
        RequireText(row.Body, 400, nameof(row.Body));
    }

    private static void RequireUtc(DateTime value, string name)
    {
        if (value.Kind == DateTimeKind.Local)
        {
            throw new ArgumentException("Times must be UTC.", name);
        }
    }

    private static void RequireDayKey(string value, string name)
    {
        if (value == null || !DateTime.TryParseExact(
                value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            throw new ArgumentException("The plan day key must be yyyy-MM-dd.", name);
        }
    }

    private static void RequireText(string value, int maxLength, string name)
    {
        if (string.IsNullOrEmpty(value) || value.Length > maxLength)
        {
            throw new ArgumentException($"The text must be 1 to {maxLength} characters.", name);
        }
    }

    private static void RequireErrorCode(string errorCode)
    {
        if (string.IsNullOrEmpty(errorCode) || errorCode.Length > 64)
        {
            throw new ArgumentException("The error code must be 1 to 64 characters.", nameof(errorCode));
        }
    }
}
