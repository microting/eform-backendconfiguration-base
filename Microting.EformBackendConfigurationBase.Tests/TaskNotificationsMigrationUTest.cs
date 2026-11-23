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
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

// The raw SQL below concatenates only literals, table names and migration ids.
#pragma warning disable EF1002, EF1003

// AddTaskNotifications: the reminder outbox table, Property.TimeZone and DeviceToken.TaskReminders,
// additive and guarded.
[TestFixture]
public class TaskNotificationsMigrationUTest : MigrationTestBase
{
    protected override string DatabaseName => "backend-configuration-pn-task-notifications-migration-tests";

    protected override string MigrationName => "AddTaskNotifications";

    private const string Table = "TaskNotifications";

    [Test]
    public void Model_HasNoChangesMissingFromTheSnapshot()
    {
        // The snapshot was merged by hand (EF 11 tooling would reformat the whole file).
        Assert.That(_dbContext.Database.HasPendingModelChanges(), Is.False);
    }

    [Test]
    public void Migration_OnEmptyDatabase_CreatesTheTableIndexesAndColumns()
    {
        _dbContext.Database.EnsureDeleted();

        MigrateTo(_migrationUnderTest);

        AssertMigrated();
    }

    [Test]
    public void Migration_FromPreviousMigration_AddsOnlyTheTableAndTheTimeZoneColumns()
    {
        var before = SchemaFingerprint();

        MigrateTo(_migrationUnderTest);
        var after = SchemaFingerprint();

        AssertMigrated();
        var added = after.Except(before).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(before.Except(after), Is.Empty, "schema rows that changed or disappeared");
            Assert.That(added.Where(row => !row.StartsWith(Table + "|", StringComparison.Ordinal)),
                Is.EquivalentTo(new[]
                {
                    after.Single(r => r.StartsWith("Properties|col|TimeZone|", StringComparison.Ordinal)),
                    after.Single(r => r.StartsWith("PropertieVersions|col|TimeZone|", StringComparison.Ordinal)),
                    after.Single(r => r.StartsWith("DeviceTokens|col|TaskReminders|", StringComparison.Ordinal)),
                    after.Single(r => r.StartsWith("DeviceTokenVersions|col|TaskReminders|", StringComparison.Ordinal))
                }),
                "outside the new table, only the TimeZone and TaskReminders columns are added");
        });
    }

    [Test]
    public void Migration_RerunAfterPartialApplication_IsANoOp()
    {
        // A pod that died after the CREATE TABLE and the first ALTER, before EF recorded the migration.
        MigrateTo(_migrationUnderTest);
        _dbContext.Database.ExecuteSqlRaw("ALTER TABLE `PropertieVersions` DROP COLUMN `TimeZone`");
        _dbContext.Database.ExecuteSqlRaw(
            "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '" + _migrationUnderTest + "'");

        Assert.That(() => MigrateTo(_migrationUnderTest), Throws.Nothing);
        AssertMigrated();
    }

    [Test]
    public void Down_RemovesTheTableAndColumns_AndNothingElse()
    {
        var before = SchemaFingerprint();
        MigrateTo(_migrationUnderTest);

        MigrateTo(_previousMigration);

        Assert.That(SchemaFingerprint(), Is.EquivalentTo(before));
    }

    // The SQL is hand-written (guarded), so pin that it creates what the model maps.
    [TestCase("TaskNotifications", "Id", "bigint(20)", "NO")]
    [TestCase("TaskNotifications", "PlanDayKey", "varchar(10)", "NO")]
    [TestCase("TaskNotifications", "ComplianceId", "int(11)", "YES")]
    [TestCase("TaskNotifications", "Kind", "varchar(16)", "NO")]
    [TestCase("TaskNotifications", "PlannedStartUtc", "datetime(6)", "NO")]
    [TestCase("TaskNotifications", "Title", "varchar(200)", "NO")]
    [TestCase("TaskNotifications", "Body", "varchar(400)", "NO")]
    [TestCase("TaskNotifications", "State", "tinyint(3) unsigned", "NO")]
    [TestCase("TaskNotifications", "LeaseUntilUtc", "datetime(6)", "YES")]
    [TestCase("TaskNotifications", "LastError", "varchar(64)", "YES")]
    [TestCase("Properties", "TimeZone", "varchar(64)", "YES")]
    [TestCase("PropertieVersions", "TimeZone", "longtext", "YES")]
    [TestCase("DeviceTokens", "TaskReminders", "tinyint(1)", "NO")]
    [TestCase("DeviceTokenVersions", "TaskReminders", "tinyint(1)", "NO")]
    public void Migration_CreatesTheTypeTheModelMaps(string table, string column, string expectedType, string expectedNullable)
    {
        MigrateTo(_migrationUnderTest);

        var where = "WHERE table_schema = DATABASE() AND table_name = '" + table + "' AND column_name = '" + column + "'";
        Assert.That(Scalar("SELECT COLUMN_TYPE FROM information_schema.COLUMNS " + where), Is.EqualTo(expectedType));
        Assert.That(Scalar("SELECT IS_NULLABLE FROM information_schema.COLUMNS " + where), Is.EqualTo(expectedNullable));
    }

    [Test]
    public void Migration_CreatesExactlyTheMappedColumns_AndUtf8mb4Text()
    {
        MigrateTo(_migrationUnderTest);

        var mapped = _dbContext.Model.FindEntityType(typeof(TaskNotification))
            .GetProperties().Select(p => p.GetColumnName()).OrderBy(c => c, StringComparer.Ordinal);
        Assert.That(Strings(
            "SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
            "AND table_name = '" + Table + "' ORDER BY COLUMN_NAME"), Is.EqualTo(mapped));
        Assert.That(Strings(
            "SELECT DISTINCT CHARACTER_SET_NAME FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
            "AND table_name = '" + Table + "' AND CHARACTER_SET_NAME IS NOT NULL"), Is.EqualTo(new[] { "utf8mb4" }));
    }

    // Existing installs (and old app versions, which never send the flag) get no task reminders.
    [Test]
    public void Migration_GivesExistingDeviceTokensTaskRemindersFalse()
    {
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `DeviceTokens` (`AppId`, `InstallationId`, `FcmToken`, `SdkSiteId`, `Platform`, `CreatedAt`, " +
            "`UpdatedAt`, `WorkflowState`, `CreatedByUserId`, `UpdatedByUserId`, `Version`) " +
            "VALUES ('microting-eform', 'install-1', 'token', 7, 'android', NOW(6), NOW(6), 'created', 0, 0, 1)");
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `DeviceTokenVersions` (`DeviceTokenId`, `AppId`, `InstallationId`, `FcmToken`, `SdkSiteId`, `Platform`, " +
            "`CreatedAt`, `UpdatedAt`, `WorkflowState`, `CreatedByUserId`, `UpdatedByUserId`, `Version`) " +
            "VALUES (1, 'microting-eform', 'install-1', 'token', 7, 'android', NOW(6), NOW(6), 'created', 0, 0, 1)");

        MigrateTo(_migrationUnderTest);

        Assert.Multiple(() =>
        {
            Assert.That(Convert.ToInt32(Scalar("SELECT `TaskReminders` FROM `DeviceTokens`")), Is.EqualTo(0));
            Assert.That(Convert.ToInt32(Scalar("SELECT `TaskReminders` FROM `DeviceTokenVersions`")), Is.EqualTo(0));
            foreach (var table in new[] { "DeviceTokens", "DeviceTokenVersions" })
            {
                Assert.That(Scalar(
                    "SELECT COLUMN_DEFAULT FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
                    "AND table_name = '" + table + "' AND column_name = 'TaskReminders'"), Is.EqualTo("0"), table);
            }
        });
        // Read only the asserted column: a whole-entity load at this fixed migration point would break
        // when a later migration adds a column to DeviceTokens.
        Assert.That(_dbContext.DeviceTokens.AsNoTracking().Select(x => x.TaskReminders).Single(), Is.False);
    }

    private void AssertMigrated()
    {
        Assert.Multiple(() =>
        {
            Assert.That(IndexColumns("PRIMARY"), Is.EqualTo(new[] { "Id" }));
            Assert.That(IndexColumns("UX_TaskNotifications_Site_Task_Day_Start_Kind"),
                Is.EqualTo(new[] { "SdkSiteId", "AreaRulePlanningId", "PlanDayKey", "PlannedStartUtc", "Kind" }));
            Assert.That(Convert.ToInt32(Scalar(
                "SELECT MAX(NON_UNIQUE) FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
                "AND table_name = '" + Table + "' AND index_name = 'UX_TaskNotifications_Site_Task_Day_Start_Kind'")),
                Is.EqualTo(0), "unique");
            Assert.That(IndexColumns("IX_TaskNotifications_State_NextAttemptAtUtc"), Is.EqualTo(new[] { "State", "NextAttemptAtUtc" }));
            Assert.That(Strings(
                    "SELECT DISTINCT INDEX_NAME FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
                    "AND table_name = '" + Table + "' ORDER BY INDEX_NAME"),
                Is.EqualTo(new[]
                {
                    "IX_TaskNotifications_CreatedAtUtc", "IX_TaskNotifications_PropertyId_PlanDayKey",
                    "IX_TaskNotifications_State_NextAttemptAtUtc", "PRIMARY", "UX_TaskNotifications_Site_Task_Day_Start_Kind"
                }), "exactly these indexes");
            Assert.That(IndexColumns("IX_TaskNotifications_PropertyId_PlanDayKey"), Is.EqualTo(new[] { "PropertyId", "PlanDayKey" }));
            Assert.That(IndexColumns("IX_TaskNotifications_CreatedAtUtc"), Is.EqualTo(new[] { "CreatedAtUtc" }));
            Assert.That(ColumnCount("Properties", "TimeZone"), Is.EqualTo(1));
            Assert.That(ColumnCount("PropertieVersions", "TimeZone"), Is.EqualTo(1));
            Assert.That(ColumnCount("DeviceTokens", "TaskReminders"), Is.EqualTo(1));
            Assert.That(ColumnCount("DeviceTokenVersions", "TaskReminders"), Is.EqualTo(1));
        });
    }

    private List<string> IndexColumns(string index) => Strings(
        "SELECT COLUMN_NAME FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
        "AND table_name = '" + Table + "' AND index_name = '" + index + "' ORDER BY SEQ_IN_INDEX");

    private int ColumnCount(string table, string column) => Convert.ToInt32(Scalar(
        "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
        "AND table_name = '" + table + "' AND column_name = '" + column + "'"));
}
