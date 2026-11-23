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
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Const;
using Infrastructure.Data;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Factories;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

// TaskChangeFeed helpers. Own database, migrated once (the feed table is not in DbTestFixture's
// truncate list, and these tests need two contexts on the same database).
[TestFixture]
public class TaskChangeFeedUTest
{
    private const string DatabaseName = "backend-configuration-pn-task-change-feed-tests";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

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
        _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE `TaskChangeFeedEntries`");
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task AppendAndSave_StoresTheKeysAndAUtcTimestamp()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var entry = await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Changed, propertyId: 42,
            areaRulePlanningId: 7, planDayKey: "2026-11-21", complianceId: 900, sdkSiteId: 3, source: "SearchListJob");

        var stored = await Fresh(db => db.TaskChangeFeedEntries.SingleAsync());
        Assert.Multiple(() =>
        {
            Assert.That(stored.Id, Is.EqualTo(entry.Id));
            Assert.That(stored.Kind, Is.EqualTo("changed"));
            Assert.That(stored.PropertyId, Is.EqualTo(42));
            Assert.That(stored.AreaRulePlanningId, Is.EqualTo(7));
            Assert.That(stored.PlanDayKey, Is.EqualTo("2026-11-21"));
            Assert.That(stored.ComplianceId, Is.EqualTo(900));
            Assert.That(stored.SdkSiteId, Is.EqualTo(3));
            Assert.That(stored.Source, Is.EqualTo("SearchListJob"));
            Assert.That(stored.CreatedAt, Is.InRange(before, DateTime.UtcNow.AddSeconds(1)));
        });
    }

    [Test]
    public async Task AppendAndSave_AllowsASiteOnlyRow()
    {
        await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Assignments, sdkSiteId: 12);

        var stored = await _dbContext.TaskChangeFeedEntries.AsNoTracking().SingleAsync();
        Assert.That(stored.PropertyId, Is.Null);
        Assert.That(stored.SdkSiteId, Is.EqualTo(12));
        Assert.That(stored.Kind, Is.EqualTo("assignments"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("Changed")]
    [TestCase("deleted")]
    public async Task AppendAndSave_RejectsAnUnknownKind(string kind)
    {
        Assert.That(async () => await TaskChangeFeed.AppendAndSaveAsync(_dbContext, kind, propertyId: 1),
            Throws.ArgumentException.With.Property("ParamName").EqualTo("kind"));
        Assert.That(_dbContext.ChangeTracker.HasChanges(), Is.False);
        Assert.That(await Fresh(db => db.TaskChangeFeedEntries.CountAsync()), Is.EqualTo(0));
    }

    [TestCase("2026-11-31")]
    [TestCase("21-11-2026")]
    [TestCase("2026-11-21T00:00")]
    [TestCase("")]
    public void AppendAndSave_RejectsAMalformedPlanDayKey(string planDayKey)
    {
        Assert.That(async () => await TaskChangeFeed.AppendAndSaveAsync(
                _dbContext, TaskChangeKinds.Changed, propertyId: 1, planDayKey: planDayKey),
            Throws.ArgumentException.With.Property("ParamName").EqualTo("planDayKey"));
    }

    [Test]
    public void AppendAndSave_RejectsASourceLongerThanTheColumn()
    {
        Assert.That(async () => await TaskChangeFeed.AppendAndSaveAsync(
                _dbContext, TaskChangeKinds.Changed, source: new string('x', 65)),
            Throws.ArgumentException.With.Property("ParamName").EqualTo("source"));
    }

    [Test]
    public async Task AppendAndSave_RefusesAContextWithPendingChanges()
    {
        _dbContext.TaskChangeFeedEntries.Add(new TaskChangeFeedEntry
            { Kind = TaskChangeKinds.Changed, CreatedAt = DateTime.UtcNow });

        Assert.That(async () => await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Changed, propertyId: 2),
            Throws.InvalidOperationException);
        Assert.That(await Fresh(db => db.TaskChangeFeedEntries.CountAsync()), Is.EqualTo(0));
    }

    // A long-lived service context must not accumulate feed rows in its change tracker.
    [Test]
    public async Task AppendAndSave_SavesAndDetachesEveryRow()
    {
        var entries = new List<TaskChangeFeedEntry>();
        for (var i = 0; i < 3; i++)
        {
            entries.Add(await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Removed, propertyId: 5, areaRulePlanningId: 8));
        }

        Assert.That(entries.Select(e => e.Id), Is.All.GreaterThan(0));
        Assert.That(entries.Select(e => _dbContext.Entry(e).State), Is.All.EqualTo(EntityState.Detached));
        Assert.That(_dbContext.ChangeTracker.Entries(), Is.Empty);
        Assert.That(await Fresh(db => db.TaskChangeFeedEntries.CountAsync()), Is.EqualTo(3));
    }

    [Test]
    public async Task ReadAfter_ReturnsRowsAfterTheCursorInIdOrder_AndRespectsMax()
    {
        var ids = new List<long>();
        for (var i = 1; i <= 5; i++)
        {
            ids.Add(await AppendAt(DateTime.UtcNow, propertyId: i));
        }

        var page = await TaskChangeFeed.ReadAfterAsync(_dbContext, ids[0], max: 3);
        Assert.That(page.Select(e => e.Id), Is.EqualTo(ids.Skip(1).Take(3)));
        Assert.That(page.Select(e => e.PropertyId), Is.EqualTo(new int?[] { 2, 3, 4 }));

        var rest = await TaskChangeFeed.ReadAfterAsync(_dbContext, page[^1].Id, max: 3);
        Assert.That(rest.Select(e => e.Id), Is.EqualTo(new[] { ids[4] }));

        Assert.That(await TaskChangeFeed.ReadAfterAsync(_dbContext, ids[4], max: 3), Is.Empty);
        Assert.That((await TaskChangeFeed.ReadAfterAsync(_dbContext, 0, max: 100)).Select(e => e.Id), Is.EqualTo(ids));
        Assert.That(_dbContext.ChangeTracker.Entries<TaskChangeFeedEntry>(), Is.Empty, "read untracked");
    }

    [Test]
    public void ReadAfter_RejectsANonPositiveMax()
    {
        Assert.That(async () => await TaskChangeFeed.ReadAfterAsync(_dbContext, 0, max: 0),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    // The load case in miniature: the reader's cursor is far ahead (many newer rows, all forwarded),
    // and a row with a low id becomes visible late. The cursor read cannot return it; the window ids
    // do, however many newer rows share the window, because ids-since has no cap.
    [Test]
    public async Task ReadIdsSince_ReturnsALateVisibleLowIdRow_EvenAmongManyNewerRows()
    {
        var old = await AppendAt(DateTime.UtcNow.AddMinutes(-1));
        var lateVisible = await AppendAt(DateTime.UtcNow.AddSeconds(-5));
        var newer = new List<long>();
        for (var i = 0; i < 200; i++)
        {
            newer.Add(await AppendAt(DateTime.UtcNow.AddSeconds(-4)));
        }

        var cursor = newer[^1];
        Assert.That(await TaskChangeFeed.ReadAfterAsync(_dbContext, cursor, max: 500), Is.Empty);

        var windowIds = await TaskChangeFeed.ReadIdsSinceAsync(_dbContext, DateTime.UtcNow.AddSeconds(-10));
        Assert.That(windowIds, Has.Count.EqualTo(201));
        Assert.That(windowIds, Is.Ordered.Ascending);
        Assert.That(windowIds[0], Is.EqualTo(lateVisible));
        Assert.That(windowIds, Has.None.EqualTo(old));

        // The reader subtracts what it already forwarded and loads the rest.
        var notForwarded = windowIds.Except(newer).ToList();
        var rows = await TaskChangeFeed.ReadByIdsAsync(_dbContext, notForwarded);
        Assert.That(rows.Select(e => e.Id), Is.EqualTo(new[] { lateVisible }));
    }

    [Test]
    public async Task ReadIdsSince_ExcludesRowsOlderThanTheWindow()
    {
        var since = DateTime.UtcNow.AddSeconds(-10);
        await AppendAt(since.AddSeconds(-30));
        await AppendAt(since.AddTicks(-10));
        var atStart = await AppendAt(since);
        var inside = await AppendAt(DateTime.UtcNow);

        Assert.That(await TaskChangeFeed.ReadIdsSinceAsync(_dbContext, since), Is.EqualTo(new[] { atStart, inside }));
    }

    [Test]
    public void ReadIdsSince_RejectsALocalTime()
    {
        Assert.That(async () => await TaskChangeFeed.ReadIdsSinceAsync(_dbContext, DateTime.Now.AddSeconds(-10)),
            Throws.ArgumentException);
    }

    [Test]
    public async Task ReadByIds_ReturnsExactlyTheRequestedRows_InIdOrder()
    {
        var ids = new List<long>();
        for (var i = 1; i <= 5; i++)
        {
            ids.Add(await AppendAt(DateTime.UtcNow, propertyId: i));
        }

        var rows = await TaskChangeFeed.ReadByIdsAsync(_dbContext, new[] { ids[3], ids[0], ids[3], 999_999 });

        Assert.That(rows.Select(e => e.Id), Is.EqualTo(new[] { ids[0], ids[3] }));
        Assert.That(rows.Select(e => e.PropertyId), Is.EqualTo(new int?[] { 1, 4 }));
        Assert.That(_dbContext.ChangeTracker.Entries<TaskChangeFeedEntry>(), Is.Empty, "read untracked");
    }

    [Test]
    public async Task ReadByIds_WithNoIds_ReturnsEmpty()
    {
        await AppendAt(DateTime.UtcNow);

        Assert.That(await TaskChangeFeed.ReadByIdsAsync(_dbContext, Array.Empty<long>()), Is.Empty);
    }

    [Test]
    public async Task GetLastId_IsZeroWhenEmpty_ThenTheHighestId()
    {
        Assert.That(await TaskChangeFeed.GetLastIdAsync(_dbContext), Is.EqualTo(0));

        await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Changed, propertyId: 1);
        var last = await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Changed, propertyId: 1);

        Assert.That(await TaskChangeFeed.GetLastIdAsync(_dbContext), Is.EqualTo(last.Id));
    }

    [Test]
    public async Task ConcurrentAppendsFromTwoContexts_GetDistinctIdsThatIncreasePerWriter()
    {
        const int perWriter = 50;

        async Task<List<long>> Write(int writer)
        {
            await using var dbContext = CreateContext();
            var ids = new List<long>();
            for (var i = 0; i < perWriter; i++)
            {
                ids.Add((await TaskChangeFeed.AppendAndSaveAsync(
                    dbContext, TaskChangeKinds.Changed, propertyId: writer, source: "writer" + writer)).Id);
            }

            return ids;
        }

        var results = await Task.WhenAll(Task.Run(() => Write(1)), Task.Run(() => Write(2)));

        Assert.Multiple(() =>
        {
            foreach (var ids in results)
            {
                Assert.That(ids, Is.Ordered.Ascending.And.Unique, "each writer sees increasing ids");
            }
        });

        var all = results.SelectMany(ids => ids).ToList();
        Assert.That(all, Is.Unique);

        var read = await TaskChangeFeed.ReadAfterAsync(_dbContext, 0, max: 1000);
        Assert.That(read, Has.Count.EqualTo(2 * perWriter));
        Assert.That(read.Select(e => e.Id), Is.Ordered.Ascending.And.Unique);
        Assert.That(read.Select(e => e.Id), Is.EquivalentTo(all));

        var cursor = read[perWriter - 1].Id;
        var after = await TaskChangeFeed.ReadAfterAsync(_dbContext, cursor, max: 1000);
        Assert.That(after.Select(e => e.Id), Is.All.GreaterThan(cursor));
        Assert.That(after, Has.Count.EqualTo(perWriter));
    }

    [Test]
    public async Task DeleteOlderThan_PhysicallyRemovesOnlyRowsBeforeTheCutoff()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var old1 = await AppendAt(cutoff.AddHours(-1));
        var old2 = await AppendAt(cutoff.AddTicks(-10)); // datetime(6): microseconds survive
        var atCutoff = await AppendAt(cutoff);
        var newer = await AppendAt(cutoff.AddHours(1));
        var now = await AppendAt(DateTime.UtcNow);

        var deleted = await TaskChangeFeed.DeleteOlderThanAsync(_dbContext, cutoff);

        Assert.That(deleted, Is.EqualTo(2));
        var remaining = await Ids();
        Assert.That(remaining, Is.EqualTo(new[] { atCutoff, newer, now }));
        Assert.That(remaining, Has.None.EqualTo(old1).And.None.EqualTo(old2));
        Assert.That(Convert.ToInt32(await RawCount()), Is.EqualTo(3), "rows are gone from the table, not flagged");
    }

    [Test]
    public async Task DeleteOlderThan_DeletesAtMostMaxRows_OldestFirst()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var ids = new List<long>();
        for (var i = 0; i < 5; i++)
        {
            ids.Add(await AppendAt(cutoff.AddHours(-10 + i)));
        }

        var fresh = await AppendAt(DateTime.UtcNow);

        Assert.That(await TaskChangeFeed.DeleteOlderThanAsync(_dbContext, cutoff, maxRows: 2), Is.EqualTo(2));
        Assert.That(await Ids(), Is.EqualTo(ids.Skip(2).Append(fresh)));

        Assert.That(await TaskChangeFeed.DeleteOlderThanAsync(_dbContext, cutoff, maxRows: 2), Is.EqualTo(2));
        Assert.That(await TaskChangeFeed.DeleteOlderThanAsync(_dbContext, cutoff, maxRows: 2), Is.EqualTo(1));
        Assert.That(await TaskChangeFeed.DeleteOlderThanAsync(_dbContext, cutoff, maxRows: 2), Is.EqualTo(0));
        Assert.That(await Ids(), Is.EqualTo(new[] { fresh }));
    }

    [Test]
    public void DeleteOlderThan_RejectsALocalCutoff()
    {
        Assert.That(async () => await TaskChangeFeed.DeleteOlderThanAsync(_dbContext, DateTime.Now.AddDays(-1)),
            Throws.ArgumentException);
    }

    // Appends through the public API, then back-dates the row (the API always stamps UtcNow).
    private async Task<long> AppendAt(DateTime createdAtUtc, int propertyId = 1)
    {
        var entry = await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Changed, propertyId: propertyId);
        await _dbContext.TaskChangeFeedEntries
            .Where(e => e.Id == entry.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.CreatedAt, createdAtUtc));
        return entry.Id;
    }

    private static Task<List<long>> Ids() =>
        Fresh(db => db.TaskChangeFeedEntries.Select(e => e.Id).OrderBy(id => id).ToListAsync());

    // Reads through a second context, so nothing comes from the first one's change tracker.
    private static async Task<T> Fresh<T>(Func<BackendConfigurationPnDbContext, Task<T>> query)
    {
        await using var dbContext = CreateContext();
        return await query(dbContext);
    }

    private async Task<object> RawCount()
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM `TaskChangeFeedEntries`";
        return await command.ExecuteScalarAsync();
    }

    private static BackendConfigurationPnDbContext CreateContext()
    {
        var dbContext = new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });
        dbContext.Database.SetCommandTimeout(600);
        return dbContext;
    }
}
