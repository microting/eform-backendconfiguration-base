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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Const;
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

// The raw SQL below concatenates only literals, table names and migration ids.
#pragma warning disable EF1002, EF1003

// AddTaskChangeFeed: additive, guarded, and the table the model maps. Own database: the tests roll
// the schema backwards, which the shared one cannot survive.
[TestFixture]
public class TaskChangeFeedMigrationUTest
{
    private const string DatabaseName = "backend-configuration-pn-task-change-feed-migration-tests";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    private const string Table = "TaskChangeFeedEntries";

    private BackendConfigurationPnDbContext _dbContext;
    private string _migrationUnderTest;
    private string _previousMigration;

    // One full migration for the fixture; each test then starts at the previous migration, and
    // moving across AddTaskChangeFeed alone is cheap.
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        using var dbContext = CreateContext();
        dbContext.Database.EnsureDeleted();
        var all = dbContext.Database.GetMigrations().ToList();
        _migrationUnderTest = all.Single(m => m.EndsWith("_AddTaskChangeFeed", StringComparison.Ordinal));
        _previousMigration = all[all.IndexOf(_migrationUnderTest) - 1];
        dbContext.GetService<IMigrator>().Migrate(_previousMigration);
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = CreateContext();
        MigrateTo(_previousMigration);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var dbContext = CreateContext();
        dbContext.Database.EnsureDeleted();
    }

    [Test]
    public void Model_HasNoChangesMissingFromTheSnapshot()
    {
        // The snapshot was merged by hand (EF 11 tooling would reformat the whole file), so pin
        // that it still describes the model exactly.
        Assert.That(_dbContext.Database.HasPendingModelChanges(), Is.False);
    }

    [Test]
    public void Migration_OnEmptyDatabase_CreatesTableWithPrimaryKeyAndCreatedAtIndex()
    {
        _dbContext.Database.EnsureDeleted();

        MigrateTo(_migrationUnderTest);

        Assert.That(TableCount(), Is.EqualTo(1));
        Assert.That(IndexColumns("PRIMARY"), Is.EqualTo(new[] { "Id" }));
        Assert.That(IndexColumns("IX_TaskChangeFeedEntries_CreatedAt"), Is.EqualTo(new[] { "CreatedAt" }));
        Assert.That(Scalar(
                "SELECT EXTRA FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
                "AND table_name = '" + Table + "' AND column_name = 'Id'"),
            Is.EqualTo("auto_increment"));
    }

    // The SQL is hand-written (guarded), so pin that it creates what the model maps: the same type,
    // length and nullability EF generates.
    [TestCase("Id", "bigint(20)", "NO")]
    [TestCase("CreatedAt", "datetime(6)", "NO")]
    [TestCase("PropertyId", "int(11)", "YES")]
    [TestCase("SdkSiteId", "int(11)", "YES")]
    [TestCase("AreaRulePlanningId", "int(11)", "YES")]
    [TestCase("PlanDayKey", "varchar(10)", "YES")]
    [TestCase("ComplianceId", "int(11)", "YES")]
    [TestCase("Kind", "varchar(16)", "NO")]
    [TestCase("Source", "varchar(64)", "YES")]
    public void Migration_CreatesTheTypeTheModelMaps(string column, string expectedType, string expectedNullable)
    {
        MigrateTo(_migrationUnderTest);

        var where = "WHERE table_schema = DATABASE() AND table_name = '" + Table + "' AND column_name = '" + column + "'";
        Assert.That(Scalar("SELECT COLUMN_TYPE FROM information_schema.COLUMNS " + where), Is.EqualTo(expectedType));
        Assert.That(Scalar("SELECT IS_NULLABLE FROM information_schema.COLUMNS " + where), Is.EqualTo(expectedNullable));
    }

    [Test]
    public void Migration_CreatesExactlyTheMappedColumns()
    {
        MigrateTo(_migrationUnderTest);

        var mapped = _dbContext.Model.FindEntityType(typeof(Infrastructure.Data.Entities.TaskChangeFeedEntry))
            .GetProperties().Select(p => p.GetColumnName()).OrderBy(c => c, StringComparer.Ordinal);
        Assert.That(Columns(Table), Is.EqualTo(mapped));
    }

    [Test]
    public void Migration_FromPreviousMigration_ChangesNoOtherTable()
    {
        MigrateTo(_previousMigration);
        var before = SchemaFingerprint();

        MigrateTo(_migrationUnderTest);
        var after = SchemaFingerprint();

        var added = after.Except(before).ToList();
        var removed = before.Except(after).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.Empty, "rows of the schema that changed or disappeared");
            Assert.That(added, Is.Not.Empty);
            Assert.That(added.Where(row => !row.StartsWith(Table + "|", StringComparison.Ordinal)
                                           && !row.StartsWith("__EFMigrationsHistory|", StringComparison.Ordinal)),
                Is.Empty, "schema rows added outside the new table");
        });
    }

    [Test]
    public void Migration_RerunAfterTheTableExists_IsANoOp()
    {
        // The state of a pod that died after the CREATE TABLE but before EF recorded the migration.
        MigrateTo(_migrationUnderTest);
        _dbContext.Database.ExecuteSqlRaw(
            "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '" + _migrationUnderTest + "'");

        Assert.That(() => MigrateTo(_migrationUnderTest), Throws.Nothing);
        Assert.That(TableCount(), Is.EqualTo(1));
    }

    [Test]
    public void Down_DropsTheTable()
    {
        MigrateTo(_migrationUnderTest);

        MigrateTo(_previousMigration);

        Assert.That(TableCount(), Is.EqualTo(0));
    }

    // A background service on the new package against a database the web plugin has not migrated
    // yet: the append fails loudly, but leaves the context usable for the caller's next save.
    [Test]
    public async Task AppendAndSave_WithoutTable_Throws_AndLeavesNoPendingChange()
    {
        MigrateTo(_previousMigration);

        Assert.That(async () => await TaskChangeFeed.AppendAndSaveAsync(
                _dbContext, TaskChangeKinds.Property, propertyId: 1, source: "test"),
            Throws.InstanceOf<DbUpdateException>());
        Assert.That(_dbContext.ChangeTracker.HasChanges(), Is.False);

        MigrateTo(_migrationUnderTest);

        var entry = await TaskChangeFeed.AppendAndSaveAsync(_dbContext, TaskChangeKinds.Property, propertyId: 1);
        Assert.That(entry.Id, Is.GreaterThan(0));
        Assert.That(_dbContext.ChangeTracker.HasChanges(), Is.False);
    }

    private static BackendConfigurationPnDbContext CreateContext()
    {
        var dbContext = new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });
        dbContext.Database.SetCommandTimeout(600);
        return dbContext;
    }

    // Always an explicit target, never Migrate() with no argument, so a later migration cannot
    // quietly take over these tests.
    private void MigrateTo(string targetMigration) =>
        _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    private int TableCount() => Convert.ToInt32(Scalar(
        "SELECT COUNT(*) FROM information_schema.TABLES WHERE table_schema = DATABASE() AND table_name = '" + Table + "'"));

    private List<string> IndexColumns(string index) => Strings(
        "SELECT COLUMN_NAME FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
        "AND table_name = '" + Table + "' AND index_name = '" + index + "' ORDER BY SEQ_IN_INDEX");

    private List<string> Columns(string table) => Strings(
        "SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
        "AND table_name = '" + table + "' ORDER BY COLUMN_NAME");

    // One row per table, column and index column: everything an ALTER of an existing table would change.
    private List<string> SchemaFingerprint() => Strings(
        "SELECT CONCAT_WS('|', TABLE_NAME, 'col', COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, " +
        "COALESCE(COLUMN_DEFAULT, '<none>'), EXTRA, COALESCE(CHARACTER_SET_NAME, ''), ORDINAL_POSITION) " +
        "FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
        "UNION ALL " +
        "SELECT CONCAT_WS('|', TABLE_NAME, 'idx', INDEX_NAME, NON_UNIQUE, SEQ_IN_INDEX, COLUMN_NAME) " +
        "FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
        "UNION ALL " +
        "SELECT CONCAT_WS('|', TABLE_NAME, 'fk', CONSTRAINT_NAME, REFERENCED_TABLE_NAME, DELETE_RULE) " +
        "FROM information_schema.REFERENTIAL_CONSTRAINTS WHERE constraint_schema = DATABASE()");

    private object Scalar(string sql)
    {
        using var command = OpenCommand(sql);
        return command.ExecuteScalar();
    }

    private List<string> Strings(string sql)
    {
        using var command = OpenCommand(sql);
        using var reader = command.ExecuteReader();
        var result = new List<string>();
        while (reader.Read())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    private IDbCommand OpenCommand(string sql)
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        var command = connection.CreateCommand();
        command.CommandText = sql;
        return command;
    }
}
