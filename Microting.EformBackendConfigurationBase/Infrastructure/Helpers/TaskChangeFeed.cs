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
using Microsoft.EntityFrameworkCore;

/// <summary>
/// The cross-process task change feed (<see cref="TaskChangeFeedEntry"/>). The background service
/// writes rows with <see cref="AppendAndSaveAsync"/>; a hosted reader in the web host polls them and
/// cleans up with <see cref="DeleteOlderThanAsync"/>. Rows are hints: a lost or duplicated row only
/// costs a client refetch.
/// <para>
/// Reader contract. Ids can become visible out of order (two writers commit in the other order;
/// Galera hands out interleaved ids per node), so a cursor alone can skip a row for good. Each poll:
/// </para>
/// <list type="number">
/// <item>Cursor read: <see cref="ReadAfterAsync"/> from the last id; forward the rows and advance the
/// cursor to the highest id returned (start at <see cref="GetLastIdAsync"/>).</item>
/// <item>Window: <see cref="ReadIdsSinceAsync"/> for <c>now - 10 s</c>, minus the ids forwarded in the
/// last ~15 s; load the remainder with <see cref="ReadByIdsAsync"/> and forward it.</item>
/// </list>
/// <para>
/// A row that becomes visible later than the window is left to the clients' own safety poll. Writer
/// and reader clocks are assumed to agree within a few seconds (CreatedAt is the writer's UTC clock).
/// </para>
/// <para>
/// Missing table: a background service may run against a database the web plugin has not migrated
/// yet. Catch the exception from AppendAndSaveAsync and log it once; the helpers never swallow
/// exceptions.
/// </para>
/// </summary>
public static class TaskChangeFeed
{
    private static readonly HashSet<string> KnownKinds =
    [
        TaskChangeKinds.Changed, TaskChangeKinds.Removed, TaskChangeKinds.Property, TaskChangeKinds.Assignments
    ];

    /// <summary>
    /// Inserts one row in its own autocommit SaveChanges and returns it, detached. Call it after your
    /// own changes are saved: it refuses a context with pending changes, so a failing feed write never
    /// takes a business write down with it and never commits someone else's half-done changes. The
    /// row is detached on success and on failure, so a long-lived context neither accumulates feed rows
    /// nor retries a failed one on its next SaveChanges. Exceptions are rethrown.
    /// </summary>
    /// <exception cref="ArgumentException">Unknown <paramref name="kind"/>, malformed
    /// <paramref name="planDayKey"/> or too long <paramref name="source"/>.</exception>
    /// <exception cref="InvalidOperationException">The context has unsaved changes.</exception>
    public static async Task<TaskChangeFeedEntry> AppendAndSaveAsync(
        BackendConfigurationPnDbContext dbContext,
        string kind,
        int? propertyId = null,
        int? areaRulePlanningId = null,
        string planDayKey = null,
        int? complianceId = null,
        int? sdkSiteId = null,
        string source = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        if (kind == null || !KnownKinds.Contains(kind))
        {
            throw new ArgumentException($"Unknown task change kind '{kind}'; use TaskChangeKinds.", nameof(kind));
        }

        if (planDayKey != null && !DateTime.TryParseExact(
                planDayKey, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            throw new ArgumentException("The plan day key must be yyyy-MM-dd.", nameof(planDayKey));
        }

        if (source is { Length: > 64 })
        {
            throw new ArgumentException("The source may be at most 64 characters.", nameof(source));
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            throw new InvalidOperationException(
                "TaskChangeFeed.AppendAndSaveAsync needs a context without unsaved changes; save your own changes first.");
        }

        var entry = new TaskChangeFeedEntry
        {
            CreatedAt = DateTime.UtcNow,
            Kind = kind,
            PropertyId = propertyId,
            AreaRulePlanningId = areaRulePlanningId,
            PlanDayKey = planDayKey,
            ComplianceId = complianceId,
            SdkSiteId = sdkSiteId,
            Source = source
        };

        dbContext.TaskChangeFeedEntries.Add(entry);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Entry(entry).State = EntityState.Detached;
        }

        return entry;
    }

    /// <summary>
    /// The highest id in the feed, or 0 when it is empty. A reader starts here: connected clients
    /// resync on connect, so older rows need no replay.
    /// </summary>
    public static async Task<long> GetLastIdAsync(
        BackendConfigurationPnDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return await dbContext.TaskChangeFeedEntries
            .MaxAsync(e => (long?)e.Id, cancellationToken)
            .ConfigureAwait(false) ?? 0;
    }

    /// <summary>
    /// Up to <paramref name="max"/> rows with <c>Id &gt; lastId</c>, in id order, untracked: the cursor
    /// read. See the class doc for the window read that catches rows visible out of order.
    /// </summary>
    public static async Task<List<TaskChangeFeedEntry>> ReadAfterAsync(
        BackendConfigurationPnDbContext dbContext,
        long lastId,
        int max,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max);
        return await dbContext.TaskChangeFeedEntries
            .AsNoTracking()
            .Where(e => e.Id > lastId)
            .OrderBy(e => e.Id)
            .Take(max)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// The ids of all rows with <c>CreatedAt &gt;= sinceUtc</c>, in id order. Deliberately uncapped
    /// (bounded by the window's length), so rows the reader already forwarded cannot crowd out one
    /// that became visible late. Served by IX_TaskChangeFeedEntries_CreatedAt.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="sinceUtc"/> is a local time.</exception>
    public static async Task<List<long>> ReadIdsSinceAsync(
        BackendConfigurationPnDbContext dbContext,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        if (sinceUtc.Kind == DateTimeKind.Local)
        {
            throw new ArgumentException("The window start must be UTC.", nameof(sinceUtc));
        }

        return await dbContext.TaskChangeFeedEntries
            .Where(e => e.CreatedAt >= sinceUtc)
            .OrderBy(e => e.Id)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// The rows with the given ids (those still present), in id order, untracked. The reader passes
    /// the window ids it has not forwarded yet. No ids: returns empty without a query.
    /// </summary>
    public static async Task<List<TaskChangeFeedEntry>> ReadByIdsAsync(
        BackendConfigurationPnDbContext dbContext,
        IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(ids);
        if (ids.Count == 0)
        {
            return [];
        }

        return await dbContext.TaskChangeFeedEntries
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Physically deletes up to <paramref name="maxRows"/> of the oldest rows created before
    /// <paramref name="cutoffUtc"/> in one set-based DELETE, and returns how many it deleted. Call it
    /// again while the result equals <paramref name="maxRows"/>; the bound keeps each statement (and
    /// each Galera write set) small.
    /// <para>
    /// Why a hard delete: everything else in this package is soft-deleted (WorkflowState = Removed).
    /// Feed rows are worthless once forwarded, and a soft-deleted log would grow forever. The product
    /// owner approved deleting rows of this table after 24 hours, an exception to the soft-delete rule
    /// for this log table only.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="cutoffUtc"/> is a local time.</exception>
    public static Task<int> DeleteOlderThanAsync(
        BackendConfigurationPnDbContext dbContext,
        DateTime cutoffUtc,
        int maxRows = 10000,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRows);
        if (cutoffUtc.Kind == DateTimeKind.Local)
        {
            throw new ArgumentException("The cut-off must be UTC.", nameof(cutoffUtc));
        }

        return dbContext.TaskChangeFeedEntries
            .Where(e => e.CreatedAt < cutoffUtc)
            .OrderBy(e => e.Id)
            .Take(maxRows)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
