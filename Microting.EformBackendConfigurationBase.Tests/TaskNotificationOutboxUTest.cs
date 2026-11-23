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

namespace Microting.EformBackendConfigurationBase.Tests;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Const;
using Infrastructure.Data;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Factories;
using Infrastructure.Enum;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;

// TaskNotificationOutbox: planner side, deliverer side, retention. Own database, migrated once.
[TestFixture]
public class TaskNotificationOutboxUTest
{
    private const string DatabaseName = "backend-configuration-pn-task-notification-outbox-tests";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(2);

    // A fixed "now" (microsecond precision, as datetime(6) stores it).
    private static readonly DateTime Now = new(2026, 11, 23, 8, 0, 0, DateTimeKind.Utc);

    private BackendConfigurationPnDbContext _dbContext;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        using var dbContext = CreateContext();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var dbContext = CreateContext();
        dbContext.Database.EnsureDeleted();
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = CreateContext();
        _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE `TaskNotifications`");
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    // ---------------------------------------------------------------- planner side

    [Test]
    public async Task Plan_InsertsPendingRows_DueAtSendAt()
    {
        var result = await TaskNotificationOutbox.PlanAsync(_dbContext,
            [Plan(site: 1, arp: 10, startInMinutes: 60, complianceId: 900), Plan(site: 2, arp: 10, startInMinutes: 60)], Now);

        AssertResult(result, inserted: 2, revived: 0);
        var rows = await All();
        Assert.That(rows, Has.Count.EqualTo(2));
        var row = rows[0];
        Assert.Multiple(() =>
        {
            Assert.That(row.State, Is.EqualTo(TaskNotificationState.Pending));
            Assert.That(row.SdkSiteId, Is.EqualTo(1));
            Assert.That(row.PropertyId, Is.EqualTo(5));
            Assert.That(row.AreaRulePlanningId, Is.EqualTo(10));
            Assert.That(row.PlanDayKey, Is.EqualTo("2026-11-23"));
            Assert.That(row.ComplianceId, Is.EqualTo(900));
            Assert.That(row.Kind, Is.EqualTo("task_due"));
            Assert.That(row.PlannedStartUtc, Is.EqualTo(Now.AddMinutes(60)));
            Assert.That(row.SendAtUtc, Is.EqualTo(Now.AddMinutes(50)));
            Assert.That(row.NextAttemptAtUtc, Is.EqualTo(row.SendAtUtc));
            Assert.That(row.Attempts, Is.EqualTo(0));
            Assert.That(row.LeaseUntilUtc, Is.Null);
            Assert.That(row.CreatedAtUtc, Is.EqualTo(Now));
            Assert.That(row.UpdatedAtUtc, Is.EqualTo(Now));
            Assert.That(rows[1].ComplianceId, Is.Null);
        });
    }

    [Test]
    public async Task Plan_IgnoresADuplicateOnTheUniqueKey()
    {
        await TaskNotificationOutbox.PlanAsync(_dbContext, [Plan(site: 1, arp: 10, startInMinutes: 60, title: "first")], Now);

        var again = await TaskNotificationOutbox.PlanAsync(_dbContext,
            [Plan(site: 1, arp: 10, startInMinutes: 60, title: "second"), Plan(site: 1, arp: 10, startInMinutes: 60, title: "third")],
            Now.AddMinutes(1));

        AssertResult(again, inserted: 0, revived: 0);
        var row = (await All()).Single();
        Assert.That(row.Title, Is.EqualTo("first"), "a duplicate changes nothing; use UpdateTextAsync");
        Assert.That(row.UpdatedAtUtc, Is.EqualTo(Now));
    }

    [Test]
    public async Task Plan_ADifferentStartTimeOrSite_IsANewReminder()
    {
        await TaskNotificationOutbox.PlanAsync(_dbContext, [Plan(site: 1, arp: 10, startInMinutes: 60)], Now);

        var result = await TaskNotificationOutbox.PlanAsync(_dbContext,
            [Plan(site: 1, arp: 10, startInMinutes: 90), Plan(site: 2, arp: 10, startInMinutes: 60)], Now);

        Assert.That(result.Inserted, Is.EqualTo(2));
        Assert.That(await All(), Has.Count.EqualTo(3));
    }

    // A task moved away and back again: the earlier row is revived instead of being lost behind the
    // unique key.
    [TestCase(TaskNotificationState.Cancelled, TaskNotificationState.Pending)]
    [TestCase(TaskNotificationState.Withdrawn, TaskNotificationState.Pending)]
    [TestCase(TaskNotificationState.WithdrawPending, TaskNotificationState.Sent)]
    [TestCase(TaskNotificationState.Pending, TaskNotificationState.Pending)]
    [TestCase(TaskNotificationState.Sent, TaskNotificationState.Sent)]
    [TestCase(TaskNotificationState.Sending, TaskNotificationState.Sending)]
    [TestCase(TaskNotificationState.Withdrawing, TaskNotificationState.Withdrawing)]
    [TestCase(TaskNotificationState.Expired, TaskNotificationState.Expired)]
    [TestCase(TaskNotificationState.Failed, TaskNotificationState.Failed)]
    public async Task Plan_AgainForAnExistingKey_RevivesOnlyRetractedRows(TaskNotificationState from, TaskNotificationState to)
    {
        var id = await PlanOne(title: "old");
        await SetState(id, from);

        var result = await TaskNotificationOutbox.PlanAsync(_dbContext, [Plan(title: "new")], Now.AddMinutes(1));

        var row = await Get(id);
        var revived = from != to;
        // Wanted afterwards: Pending, Sending, Sent (or finished by time: Expired, Failed). A row still
        // being withdrawn must be planned again.
        var retry = from == TaskNotificationState.Withdrawing;
        AssertResult(result, inserted: 0, revived: revived ? 1 : 0, retry: retry ? [id] : []);
        Assert.That(row.State, Is.EqualTo(to));
        if (to == TaskNotificationState.Pending && revived)
        {
            Assert.That(row.Title, Is.EqualTo("new"));
            Assert.That(row.NextAttemptAtUtc, Is.EqualTo(row.SendAtUtc));
            Assert.That(row.LastError, Is.Null);
            Assert.That(row.WithdrawnAtUtc, Is.Null);
        }
    }

    // Moved 09:00 -> 10:00 -> 09:00 while the deliverer is withdrawing the 09:00 reminder: the
    // planner must learn the row is in flight, keep the property dirty and plan again once the
    // withdraw has finished, which then revives the row.
    [Test]
    public async Task Plan_ReportsAnInFlightRowOfAPlannedKey_SoThePlannerCanRetry()
    {
        var nine = await PlanOne(startInMinutes: 60);
        var claim = await ClaimOne(arp: 10, existingId: nine, sentFirst: true);
        Assert.That(claim.State, Is.EqualTo(TaskNotificationState.Withdrawing));

        var replan = await TaskNotificationOutbox.PlanAsync(_dbContext, [Plan(startInMinutes: 60)], Now.AddMinutes(1));

        AssertResult(replan, inserted: 0, revived: 0, retry: [nine]);

        Assert.That(await TaskNotificationOutbox.MarkWithdrawnAsync(_dbContext, claim, Now.AddMinutes(1)), Is.True);
        var retry = await TaskNotificationOutbox.PlanAsync(_dbContext, [Plan(startInMinutes: 60)], Now.AddMinutes(2));

        AssertResult(retry, inserted: 0, revived: 1);
        Assert.That((await Get(nine)).State, Is.EqualTo(TaskNotificationState.Pending));
    }

    [Test]
    public void Plan_RejectsInvalidRows()
    {
        var bad = new Dictionary<string, TaskNotificationPlan>
        {
            ["SdkSiteId"] = Plan() with { SdkSiteId = 0 },
            ["PropertyId"] = Plan() with { PropertyId = -1 },
            ["AreaRulePlanningId"] = Plan() with { AreaRulePlanningId = 0 },
            ["ComplianceId"] = Plan() with { ComplianceId = 0 },
            ["PlanDayKey"] = Plan() with { PlanDayKey = "2026-13-01" },
            ["Kind"] = Plan() with { Kind = "task_late" },
            ["Title"] = Plan() with { Title = new string('x', 201) },
            ["Title "] = Plan() with { Title = "" },
            ["Body"] = Plan() with { Body = new string('x', 401) },
            ["Body "] = Plan() with { Body = null },
            ["SendAtUtc"] = Plan() with { SendAtUtc = Now.AddMinutes(61) },
            ["PlannedStartUtc"] = Plan() with { PlannedStartUtc = DateTime.SpecifyKind(Now.AddHours(1), DateTimeKind.Local) },
        };

        Assert.Multiple(() =>
        {
            foreach (var (name, plan) in bad)
            {
                Assert.That(async () => await TaskNotificationOutbox.PlanAsync(_dbContext, [plan], Now),
                    Throws.InstanceOf<ArgumentException>(), name);
            }

            Assert.That(async () => await TaskNotificationOutbox.PlanAsync(_dbContext, [Plan()], DateTime.Now),
                Throws.ArgumentException, "local now");
        });
        Assert.That(_dbContext.TaskNotifications.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task CancelOrWithdraw_CancelsPending_WithdrawsSent_AndSkipsInFlightAndFinishedRows()
    {
        var ids = new Dictionary<TaskNotificationState, long>();
        var minute = 0;
        foreach (var state in System.Enum.GetValues<TaskNotificationState>())
        {
            ids[state] = await PlanOne(arp: 100 + minute++);
            await SetState(ids[state], state);
        }

        var result = await TaskNotificationOutbox.CancelOrWithdrawAsync(_dbContext, ids.Values.ToList(), Now.AddMinutes(5));

        Assert.That(result.Cancelled, Is.EqualTo(1));
        Assert.That(result.WithdrawRequested, Is.EqualTo(1));
        // Not retracted yet: only the row being sent (a withdraw in flight is already on its way out).
        Assert.That(result.RetryIds, Is.EqualTo(new[] { ids[TaskNotificationState.Sending] }));

        await Assert.MultipleAsync(async () =>
        {
            Assert.That((await Get(ids[TaskNotificationState.Pending])).State, Is.EqualTo(TaskNotificationState.Cancelled));
            var withdraw = await Get(ids[TaskNotificationState.Sent]);
            Assert.That(withdraw.State, Is.EqualTo(TaskNotificationState.WithdrawPending));
            Assert.That(withdraw.NextAttemptAtUtc, Is.EqualTo(Now.AddMinutes(5)), "a withdraw is due at once");
            Assert.That(withdraw.UpdatedAtUtc, Is.EqualTo(Now.AddMinutes(5)));
            foreach (var state in new[]
                     {
                         TaskNotificationState.Sending, TaskNotificationState.WithdrawPending, TaskNotificationState.Withdrawing,
                         TaskNotificationState.Withdrawn, TaskNotificationState.Cancelled, TaskNotificationState.Expired,
                         TaskNotificationState.Failed
                     })
            {
                Assert.That((await Get(ids[state])).State, Is.EqualTo(state), state.ToString());
            }
        });
    }

    [Test]
    public void CancelOrWithdraw_RejectsNonPositiveIds()
    {
        Assert.That(async () => await TaskNotificationOutbox.CancelOrWithdrawAsync(_dbContext, [1, 0], Now),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public async Task CancelOrWithdraw_WithNoIds_DoesNothing()
    {
        var result = await TaskNotificationOutbox.CancelOrWithdrawAsync(_dbContext, [], Now);

        Assert.That(result.Cancelled + result.WithdrawRequested, Is.EqualTo(0));
        Assert.That(result.RetryIds, Is.Empty);
    }

    [Test]
    public async Task UpdateText_ChangesPendingRowsOnly()
    {
        var pending = await PlanOne(arp: 1);
        var sent = await PlanOne(arp: 2);
        await SetState(sent, TaskNotificationState.Sent);

        Assert.That(await TaskNotificationOutbox.UpdateTextAsync(_dbContext, pending, "Ny titel", "kl. 09:30", Now.AddMinutes(2)), Is.True);
        Assert.That(await TaskNotificationOutbox.UpdateTextAsync(_dbContext, sent, "Ny titel", "kl. 09:30", Now.AddMinutes(2)), Is.False);

        var row = await Get(pending);
        Assert.That(row.Title, Is.EqualTo("Ny titel"));
        Assert.That(row.Body, Is.EqualTo("kl. 09:30"));
        Assert.That(row.UpdatedAtUtc, Is.EqualTo(Now.AddMinutes(2)));
        Assert.That((await Get(sent)).Title, Is.EqualTo("Title"));
        Assert.That(async () => await TaskNotificationOutbox.UpdateTextAsync(_dbContext, pending, new string('x', 201), "b", Now),
            Throws.ArgumentException);
    }

    [Test]
    public async Task List_ReturnsTheRowsOfAProperty_ForOneDay_AsUtc()
    {
        await TaskNotificationOutbox.PlanAsync(_dbContext,
        [
            Plan(site: 1, arp: 10, property: 5),
            Plan(site: 1, arp: 11, property: 5),
            Plan(site: 1, arp: 12, property: 6),
            Plan(site: 1, arp: 10, property: 5, day: "2026-11-24", startInMinutes: 24 * 60)
        ], Now);

        var byProperty = await TaskNotificationOutbox.ListForPropertyDayAsync(_dbContext, 5, "2026-11-23");
        Assert.That(byProperty.Select(r => r.AreaRulePlanningId), Is.EquivalentTo(new[] { 10, 11 }));

        AssertUtcKinds(byProperty[0]);
        Assert.That(_dbContext.ChangeTracker.Entries(), Is.Empty, "read untracked");
    }

    // ---------------------------------------------------------------- deliverer side

    [Test]
    public async Task Claim_TakesDueRowsOnly_InDueOrder_UpToMax()
    {
        var late = await PlanOne(arp: 1, startInMinutes: 12);   // due at now + 2 min: not yet
        var due1 = await PlanOne(arp: 2, startInMinutes: 5);    // due 5 min ago
        var due2 = await PlanOne(arp: 3, startInMinutes: 8);    // due 2 min ago
        var due3 = await PlanOne(arp: 4, startInMinutes: 9);    // due 1 min ago
        var sent = await PlanOne(arp: 5, startInMinutes: 1);
        await SetState(sent, TaskNotificationState.Sent);

        var claimed = await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now, max: 2, Lease);

        Assert.That(claimed.Select(c => c.Id), Is.EqualTo(new[] { due1, due2 }));
        await Assert.MultipleAsync(async () =>
        {
            foreach (var c in claimed)
            {
                Assert.That(c.State, Is.EqualTo(TaskNotificationState.Sending));
                Assert.That(c.LeaseUntilUtc, Is.EqualTo(Now + Lease));
                Assert.That(c.Attempts, Is.EqualTo(1));
                AssertUtcKinds(c);
                var stored = await Get(c.Id);
                Assert.That(stored.State, Is.EqualTo(TaskNotificationState.Sending));
                Assert.That(stored.Attempts, Is.EqualTo(1));
            }

            Assert.That((await Get(due3)).State, Is.EqualTo(TaskNotificationState.Pending));
            Assert.That((await Get(late)).State, Is.EqualTo(TaskNotificationState.Pending));
        });

        var rest = await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now, max: 10, Lease);
        Assert.That(rest.Select(c => c.Id), Is.EqualTo(new[] { due3 }));
        Assert.That(_dbContext.ChangeTracker.Entries(), Is.Empty, "claimed rows come back untracked");
    }

    [Test]
    public async Task Claim_TakesAWithdrawAsWithdrawing()
    {
        var id = await PlanOne();
        await SetState(id, TaskNotificationState.Sent);
        await TaskNotificationOutbox.CancelOrWithdrawAsync(_dbContext, [id], Now);

        var claimed = (await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now, 10, Lease)).Single();

        Assert.That(claimed.State, Is.EqualTo(TaskNotificationState.Withdrawing));
        Assert.That((await Get(id)).State, Is.EqualTo(TaskNotificationState.Withdrawing));
    }

    // Two deliverers (two contexts, two connections) race for the same due rows: every row is claimed
    // exactly once.
    [Test]
    public async Task Claim_FromTwoContextsConcurrently_ClaimsEachRowExactlyOnce()
    {
        const int rows = 60;
        var plans = Enumerable.Range(1, rows).Select(i => Plan(site: 1, arp: i, startInMinutes: 5)).ToList();
        await TaskNotificationOutbox.PlanAsync(_dbContext, plans, Now);

        async Task<List<long>> Deliverer()
        {
            await using var dbContext = CreateContext();
            var mine = new List<long>();
            while (true)
            {
                var claimed = await TaskNotificationOutbox.ClaimDueAsync(dbContext, Now, max: 7, Lease);
                if (claimed.Count == 0)
                {
                    return mine;
                }

                mine.AddRange(claimed.Select(c => c.Id));
            }
        }

        var results = await Task.WhenAll(Task.Run(Deliverer), Task.Run(Deliverer));

        var all = results.SelectMany(r => r).ToList();
        Assert.That(all, Has.Count.EqualTo(rows));
        Assert.That(all, Is.Unique, "no row claimed twice");
        Assert.That(results[0].Intersect(results[1]), Is.Empty);
        Assert.That(_dbContext.TaskNotifications.Count(r => r.State == TaskNotificationState.Sending && r.Attempts == 1),
            Is.EqualTo(rows));
    }

    [Test]
    public async Task Reap_ReturnsExpiredLeasesToTheirQueue_AndLeavesLiveLeases()
    {
        var send = await PlanOne(arp: 1, startInMinutes: 5);
        var withdraw = await PlanOne(arp: 2, startInMinutes: 5);
        await SetState(withdraw, TaskNotificationState.Sent);
        await TaskNotificationOutbox.CancelOrWithdrawAsync(_dbContext, [withdraw], Now);
        var claimed = await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now, 10, Lease);
        Assert.That(claimed, Has.Count.EqualTo(2));

        Assert.That(await TaskNotificationOutbox.ReapExpiredLeasesAsync(_dbContext, Now + Lease), Is.EqualTo(0), "lease still live at its end");

        Assert.That(await TaskNotificationOutbox.ReapExpiredLeasesAsync(_dbContext, Now + Lease + TimeSpan.FromSeconds(1)), Is.EqualTo(2));
        var sendRow = await Get(send);
        Assert.That(sendRow.State, Is.EqualTo(TaskNotificationState.Pending));
        Assert.That(sendRow.LeaseUntilUtc, Is.Null);
        Assert.That(sendRow.Attempts, Is.EqualTo(1), "the attempt counts");
        Assert.That((await Get(withdraw)).State, Is.EqualTo(TaskNotificationState.WithdrawPending));

        var reclaimed = await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now + Lease + TimeSpan.FromSeconds(2), 10, Lease);
        Assert.That(reclaimed.Select(c => c.Attempts), Is.All.EqualTo(2));
    }

    [Test]
    public async Task MarkSent_FromTheClaim()
    {
        var claim = await ClaimOne();

        Assert.That(await TaskNotificationOutbox.MarkSentAsync(_dbContext, claim, Now.AddSeconds(3)), Is.True);

        var row = await Get(claim.Id);
        Assert.That(row.State, Is.EqualTo(TaskNotificationState.Sent));
        Assert.That(row.SentAtUtc, Is.EqualTo(Now.AddSeconds(3)));
        Assert.That(row.LeaseUntilUtc, Is.Null);
        Assert.That(row.UpdatedAtUtc, Is.EqualTo(Now.AddSeconds(3)));
    }

    // A deliverer whose lease was reaped and whose row another deliverer claimed again must not
    // overwrite the newer claim: the attempt number is the fencing token.
    [Test]
    public async Task Marks_FromAStaleClaim_AreRefused()
    {
        var stale = await ClaimOne();
        await TaskNotificationOutbox.ReapExpiredLeasesAsync(_dbContext, Now + Lease + TimeSpan.FromSeconds(1));
        var fresh = (await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now + Lease + TimeSpan.FromSeconds(1), 10, Lease)).Single();

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await TaskNotificationOutbox.MarkSentAsync(_dbContext, stale, Now), Is.False);
            Assert.That(await TaskNotificationOutbox.MarkRetryAsync(_dbContext, stale, Now.AddSeconds(4), "UNAVAILABLE", Now), Is.False);
            Assert.That(await TaskNotificationOutbox.MarkExpiredAsync(_dbContext, stale, Now), Is.False);
            Assert.That(await TaskNotificationOutbox.MarkFailedAsync(_dbContext, stale, "INTERNAL", Now), Is.False);
            var row = await Get(fresh.Id);
            Assert.That(row.State, Is.EqualTo(TaskNotificationState.Sending));
            Assert.That(row.Attempts, Is.EqualTo(2));
            Assert.That(row.LeaseUntilUtc, Is.EqualTo(fresh.LeaseUntilUtc));
        });

        Assert.That(await TaskNotificationOutbox.MarkSentAsync(_dbContext, fresh, Now.AddMinutes(3)), Is.True);
    }

    // The planner cannot touch an in-flight row, so a mark never meets a row the planner changed;
    // but a mark after another mark (a repeated call) is refused.
    [Test]
    public async Task Marks_AfterAnotherMark_AreRefused()
    {
        var claim = await ClaimOne();
        Assert.That(await TaskNotificationOutbox.MarkSentAsync(_dbContext, claim, Now), Is.True);

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await TaskNotificationOutbox.MarkSentAsync(_dbContext, claim, Now), Is.False);
            Assert.That(await TaskNotificationOutbox.MarkRetryAsync(_dbContext, claim, Now, "X", Now), Is.False);
            Assert.That(await TaskNotificationOutbox.MarkExpiredAsync(_dbContext, claim, Now), Is.False);
            Assert.That(await TaskNotificationOutbox.MarkFailedAsync(_dbContext, claim, "X", Now), Is.False);
            Assert.That((await Get(claim.Id)).State, Is.EqualTo(TaskNotificationState.Sent));
        });
    }

    [Test]
    public async Task Marks_ForTheOtherDirection_AreRefused()
    {
        var sending = await ClaimOne(arp: 1);
        var withdrawing = await ClaimOne(arp: 2, withdraw: true);

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await TaskNotificationOutbox.MarkWithdrawnAsync(_dbContext, sending, Now), Is.False, "withdrawn from Sending");
            Assert.That(await TaskNotificationOutbox.MarkSentAsync(_dbContext, withdrawing, Now), Is.False, "sent from Withdrawing");
            Assert.That(await TaskNotificationOutbox.MarkExpiredAsync(_dbContext, withdrawing, Now), Is.False, "expired from Withdrawing");
            Assert.That((await Get(sending.Id)).State, Is.EqualTo(TaskNotificationState.Sending));
            Assert.That((await Get(withdrawing.Id)).State, Is.EqualTo(TaskNotificationState.Withdrawing));
        });
    }

    [Test]
    public async Task MarkWithdrawn_FromTheClaim()
    {
        var claim = await ClaimOne(withdraw: true);

        Assert.That(await TaskNotificationOutbox.MarkWithdrawnAsync(_dbContext, claim, Now.AddSeconds(2)), Is.True);

        var row = await Get(claim.Id);
        Assert.That(row.State, Is.EqualTo(TaskNotificationState.Withdrawn));
        Assert.That(row.WithdrawnAtUtc, Is.EqualTo(Now.AddSeconds(2)));
        Assert.That(row.LeaseUntilUtc, Is.Null);
    }

    [TestCase(false, TaskNotificationState.Pending)]
    [TestCase(true, TaskNotificationState.WithdrawPending)]
    public async Task MarkRetry_ReturnsTheRowToItsQueue_WithBackOffAndErrorCode(bool withdraw, TaskNotificationState queue)
    {
        var claim = await ClaimOne(withdraw: withdraw);

        Assert.That(await TaskNotificationOutbox.MarkRetryAsync(_dbContext, claim, Now.AddSeconds(4), "UNAVAILABLE", Now.AddSeconds(1)), Is.True);

        var row = await Get(claim.Id);
        Assert.Multiple(() =>
        {
            Assert.That(row.State, Is.EqualTo(queue));
            Assert.That(row.NextAttemptAtUtc, Is.EqualTo(Now.AddSeconds(4)));
            Assert.That(row.LastError, Is.EqualTo("UNAVAILABLE"));
            Assert.That(row.LeaseUntilUtc, Is.Null);
            Assert.That(row.Attempts, Is.EqualTo(1));
        });
        Assert.That(await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now.AddSeconds(3), 10, Lease), Is.Empty, "backing off");
        Assert.That((await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now.AddSeconds(4), 10, Lease)).Single().Attempts, Is.EqualTo(2));
    }

    [Test]
    public async Task MarkExpired_AndMarkFailed_EndTheRow()
    {
        var expire = await ClaimOne(arp: 1);
        var failSend = await ClaimOne(arp: 2);
        var failWithdraw = await ClaimOne(arp: 3, withdraw: true);

        Assert.That(await TaskNotificationOutbox.MarkExpiredAsync(_dbContext, expire, Now), Is.True);
        Assert.That(await TaskNotificationOutbox.MarkFailedAsync(_dbContext, failSend, "SENDER_ID_MISMATCH", Now), Is.True);
        Assert.That(await TaskNotificationOutbox.MarkFailedAsync(_dbContext, failWithdraw, "WITHDRAW_TOO_LATE", Now), Is.True);

        Assert.That((await Get(expire.Id)).State, Is.EqualTo(TaskNotificationState.Expired));
        var failed = await Get(failSend.Id);
        Assert.That(failed.State, Is.EqualTo(TaskNotificationState.Failed));
        Assert.That(failed.LastError, Is.EqualTo("SENDER_ID_MISMATCH"));
        Assert.That((await Get(failWithdraw.Id)).State, Is.EqualTo(TaskNotificationState.Failed));
        Assert.That(await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now.AddDays(1), 10, Lease), Is.Empty);
    }

    [Test]
    public async Task Deliverer_RejectsInvalidArguments()
    {
        var claim = await ClaimOne();
        Assert.Multiple(() =>
        {
            Assert.That(async () => await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now, 0, Lease), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(async () => await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now, 5, TimeSpan.Zero), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(async () => await TaskNotificationOutbox.ClaimDueAsync(_dbContext, DateTime.Now, 5, Lease), Throws.ArgumentException);
            Assert.That(async () => await TaskNotificationOutbox.ReapExpiredLeasesAsync(_dbContext, DateTime.Now), Throws.ArgumentException);
            Assert.That(async () => await TaskNotificationOutbox.MarkSentAsync(_dbContext, null, Now), Throws.ArgumentNullException);
            Assert.That(async () => await TaskNotificationOutbox.MarkRetryAsync(_dbContext, claim, Now, "", Now), Throws.ArgumentException);
            Assert.That(async () => await TaskNotificationOutbox.MarkRetryAsync(_dbContext, claim, Now, new string('E', 65), Now), Throws.ArgumentException);
            Assert.That(async () => await TaskNotificationOutbox.MarkRetryAsync(_dbContext, claim, DateTime.Now, "E", Now), Throws.ArgumentException);
            Assert.That(async () => await TaskNotificationOutbox.MarkFailedAsync(_dbContext, claim, null, Now), Throws.ArgumentException);
        });
        Assert.That((await Get(claim.Id)).State, Is.EqualTo(TaskNotificationState.Sending));
    }

    // ---------------------------------------------------------------- interleavings (PR #894 review)
    // A command interceptor on the helper's context runs a competing deliverer step on another
    // connection right before the helper's N-th query, i.e. between two of the helper's statements.

    // The withdraw finishes between the revival updates and the final read: the wanted 09:00
    // reminder is Withdrawn, so it must be reported for another pass.
    [Test]
    public async Task Plan_WhenAWithdrawFinishesBetweenItsSteps_ReportsTheRowForRetry()
    {
        var nine = await PlanOne(startInMinutes: 60);
        var claim = await ClaimOne(existingId: nine, sentFirst: true);

        var interleave = new Interleave(readerNumber: 1,
            () => WithFresh(db => TaskNotificationOutbox.MarkWithdrawnAsync(db, claim, Now.AddMinutes(1))));
        await using var planner = CreateContext(interleave);
        var result = await TaskNotificationOutbox.PlanAsync(planner, [Plan(startInMinutes: 60)], Now.AddMinutes(1));

        Assert.That(interleave.Ran, Is.True);
        Assert.That((await Get(nine)).State, Is.EqualTo(TaskNotificationState.Withdrawn));
        AssertResult(result, inserted: 0, revived: 0, retry: [nine]);
    }

    // The send finishes (Sent) or backs off (Pending) after the retraction updates: the unwanted
    // reminder is still live, so it must be reported for another pass.
    [TestCase(true)]
    [TestCase(false)]
    public async Task CancelOrWithdraw_WhenASendEndsBetweenItsSteps_ReportsTheRowForRetry(bool sent)
    {
        var claim = await ClaimOne();

        var interleave = new Interleave(readerNumber: 1, () => WithFresh(db => sent
            ? TaskNotificationOutbox.MarkSentAsync(db, claim, Now)
            : TaskNotificationOutbox.MarkRetryAsync(db, claim, Now.AddSeconds(4), "UNAVAILABLE", Now)));
        await using var planner = CreateContext(interleave);
        var result = await TaskNotificationOutbox.CancelOrWithdrawAsync(planner, [claim.Id], Now);

        Assert.That(interleave.Ran, Is.True);
        Assert.That((await Get(claim.Id)).State, Is.EqualTo(sent ? TaskNotificationState.Sent : TaskNotificationState.Pending));
        Assert.That(result.Cancelled + result.WithdrawRequested, Is.EqualTo(0));
        Assert.That(result.RetryIds, Is.EqualTo(new[] { claim.Id }));
    }

    // Deliverer A wins the claim UPDATE, then pauses; its lease is reaped and B claims the row before
    // A reads its claim back. A must get nothing for that row (B's token is not A's).
    [Test]
    public async Task Claim_WhenTheRowIsReclaimedBeforeTheReadBack_ReturnsNothingForIt()
    {
        var id = await PlanOne(startInMinutes: 5);
        TaskNotification bClaim = null;
        var later = Now + Lease + TimeSpan.FromSeconds(1);

        var interleave = new Interleave(readerNumber: 2, async () =>
        {
            await WithFresh(db => TaskNotificationOutbox.ReapExpiredLeasesAsync(db, later));
            bClaim = (await WithFresh(db => TaskNotificationOutbox.ClaimDueAsync(db, later, 10, Lease))).Single();
        });
        await using var deliverer = CreateContext(interleave);
        var aClaims = await TaskNotificationOutbox.ClaimDueAsync(deliverer, Now, 10, Lease);

        Assert.That(interleave.Ran, Is.True);
        Assert.That(aClaims, Is.Empty, "A must not receive B's claim");
        Assert.That(bClaim.Id, Is.EqualTo(id));
        Assert.That(bClaim.Attempts, Is.EqualTo(2));
        Assert.That(await TaskNotificationOutbox.MarkSentAsync(_dbContext, bClaim, later), Is.True);
    }

    // ---------------------------------------------------------------- retention and text

    [Test]
    public async Task DeleteOlderThan_RemovesOnlyRowsCreatedBeforeTheCutoff_AtMostMaxRows()
    {
        var cutoff = Now.AddDays(-30);
        var old = new List<long>();
        for (var i = 0; i < 3; i++)
        {
            old.Add(await PlanOne(arp: i + 1, now: cutoff.AddHours(-3 + i)));
            await SetState(old[^1], TaskNotificationState.Sent);
        }

        var atCutoff = await PlanOne(arp: 10, now: cutoff);
        await SetState(atCutoff, TaskNotificationState.Sent);
        var fresh = await PlanOne(arp: 11, now: Now);

        Assert.That(await TaskNotificationOutbox.DeleteOlderThanAsync(_dbContext, cutoff, maxRows: 2), Is.EqualTo(2));
        Assert.That(await Ids(), Is.EqualTo(new[] { old[2], atCutoff, fresh }));
        Assert.That(await TaskNotificationOutbox.DeleteOlderThanAsync(_dbContext, cutoff, maxRows: 2), Is.EqualTo(1));
        Assert.That(await TaskNotificationOutbox.DeleteOlderThanAsync(_dbContext, cutoff, maxRows: 2), Is.EqualTo(0));
        Assert.That(await Ids(), Is.EqualTo(new[] { atCutoff, fresh }));
        Assert.That(async () => await TaskNotificationOutbox.DeleteOlderThanAsync(_dbContext, DateTime.Now), Throws.ArgumentException);
    }

    // Only finished rows are deletable; a live row is kept whatever its age.
    [TestCase(TaskNotificationState.Sent, true)]
    [TestCase(TaskNotificationState.Withdrawn, true)]
    [TestCase(TaskNotificationState.Cancelled, true)]
    [TestCase(TaskNotificationState.Expired, true)]
    [TestCase(TaskNotificationState.Failed, true)]
    [TestCase(TaskNotificationState.Pending, false)]
    [TestCase(TaskNotificationState.Sending, false)]
    [TestCase(TaskNotificationState.WithdrawPending, false)]
    [TestCase(TaskNotificationState.Withdrawing, false)]
    public async Task DeleteOlderThan_DeletesOnlyFinishedRows(TaskNotificationState state, bool deletable)
    {
        var id = await PlanOne(now: Now.AddDays(-60));
        await SetState(id, state);

        Assert.That(await TaskNotificationOutbox.DeleteOlderThanAsync(_dbContext, Now.AddDays(-30)), Is.EqualTo(deletable ? 1 : 0));
        Assert.That(await Ids(), deletable ? Is.Empty : Is.EqualTo(new[] { id }));
    }

    [TestCase("Rengør stald 🐷 – kl. 07:00", "Æbler, øl og ål")]
    [TestCase("Καθαρισμός στάβλου", "Ώρα 07:00 — 🧹✨")]
    public async Task Text_StoresDanishGreekAndEmoji(string title, string body)
    {
        var id = await PlanOne(title: title, body: body);

        var row = await Get(id);
        Assert.That(row.Title, Is.EqualTo(title));
        Assert.That(row.Body, Is.EqualTo(body));
    }

    // ---------------------------------------------------------------- helpers

    private static TaskNotificationPlan Plan(int site = 1, int arp = 10, int property = 5, int startInMinutes = 60,
        int? complianceId = null, string title = "Title", string body = "Body", string day = "2026-11-23")
        => new(site, property, arp, day, complianceId, TaskNotificationKinds.TaskDue,
            Now.AddMinutes(startInMinutes), Now.AddMinutes(startInMinutes - 10), title, body);

    private async Task<long> PlanOne(int arp = 10, int startInMinutes = 60, string title = "Title", string body = "Body",
        DateTime? now = null)
    {
        await TaskNotificationOutbox.PlanAsync(_dbContext,
            [Plan(arp: arp, startInMinutes: startInMinutes, title: title, body: body)], now ?? Now);
        return await _dbContext.TaskNotifications.Where(r => r.AreaRulePlanningId == arp).Select(r => r.Id).SingleAsync();
    }

    // Arrange a state directly (the tests of each transition use the API).
    private Task SetState(long id, TaskNotificationState state) =>
        _dbContext.TaskNotifications.Where(r => r.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.State, state).SetProperty(r => r.NextAttemptAtUtc, Now));

    private async Task<TaskNotification> ClaimOne(int arp = 10, bool withdraw = false, long? existingId = null,
        bool sentFirst = false)
    {
        var id = existingId ?? await PlanOne(arp: arp, startInMinutes: 5);
        if (sentFirst)
        {
            // Sent, then retracted by the planner: due as a withdraw.
            await SetState(id, TaskNotificationState.Sent);
            await TaskNotificationOutbox.CancelOrWithdrawAsync(_dbContext, [id], Now);
        }
        else if (withdraw)
        {
            await SetState(id, TaskNotificationState.WithdrawPending);
        }

        var claimed = await TaskNotificationOutbox.ClaimDueAsync(_dbContext, Now, 100, Lease);
        return claimed.Single(c => c.Id == id);
    }

    private static void AssertResult(TaskNotificationPlanResult result, int inserted, int revived, long[] retry = null)
    {
        Assert.That(result.Inserted, Is.EqualTo(inserted), "inserted");
        Assert.That(result.Revived, Is.EqualTo(revived), "revived");
        Assert.That(result.RetryIds, Is.EqualTo(retry ?? []), "retry");
    }

    // Rows the helper returns carry UTC kinds, like the values that went in.
    private static void AssertUtcKinds(TaskNotification row)
    {
        Assert.Multiple(() =>
        {
            Assert.That(row.PlannedStartUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "PlannedStartUtc");
            Assert.That(row.SendAtUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "SendAtUtc");
            Assert.That(row.NextAttemptAtUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "NextAttemptAtUtc");
            Assert.That(row.CreatedAtUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "CreatedAtUtc");
            Assert.That(row.UpdatedAtUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "UpdatedAtUtc");
            if (row.LeaseUntilUtc is { } lease)
            {
                Assert.That(lease.Kind, Is.EqualTo(DateTimeKind.Utc), "LeaseUntilUtc");
            }
        });
    }

    private async Task<TaskNotification> Get(long id)
    {
        await using var dbContext = CreateContext();
        return await dbContext.TaskNotifications.SingleAsync(r => r.Id == id);
    }

    private async Task<List<TaskNotification>> All()
    {
        await using var dbContext = CreateContext();
        return await dbContext.TaskNotifications.OrderBy(r => r.Id).ToListAsync();
    }

    private async Task<List<long>> Ids() => (await All()).Select(r => r.Id).ToList();

    private static async Task<T> WithFresh<T>(Func<BackendConfigurationPnDbContext, Task<T>> action)
    {
        await using var dbContext = CreateContext();
        return await action(dbContext);
    }

    // A context whose N-th reader command first runs a competing step (on other connections).
    private static BackendConfigurationPnDbContext CreateContext(Interleave interleave)
    {
        var options = new DbContextOptionsBuilder<BackendConfigurationPnDbContext>()
            .UseMySql(ConnectionString, new MariaDbServerVersion(ServerVersion.AutoDetect(ConnectionString)))
            .AddInterceptors(interleave)
            .Options;
        return new BackendConfigurationPnDbContext(options);
    }

    // Test seam only: runs Hook once, right before the ReaderNumber-th query of the context.
    private sealed class Interleave(int readerNumber, Func<Task> hook) : DbCommandInterceptor
    {
        private int _readers;

        public bool Ran { get; private set; }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (++_readers == readerNumber && !Ran)
            {
                Ran = true;
                await hook();
            }

            return result;
        }
    }

    private static BackendConfigurationPnDbContext CreateContext()
    {
        var dbContext = new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });
        dbContext.Database.SetCommandTimeout(600);
        return dbContext;
    }
}
