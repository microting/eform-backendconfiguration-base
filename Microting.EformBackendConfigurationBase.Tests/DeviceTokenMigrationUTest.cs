/*
The MIT License (MIT)

Copyright (c) 2007 - 2022 Microting A/S

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
using System.Data;
using System.Linq;
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

// DbTestFixture migrates a fresh database, so the backfill in
// DeviceTokenIdentityModel matches zero rows there and its whole data path is
// untested. These tests migrate to the migration BEFORE it, insert rows at the
// old schema - including the shape that already failed for real, two rows
// sharing an FcmToken - and only then migrate forward.
//
// Own database: the tests roll the schema backwards, which the shared one
// cannot survive.
[TestFixture]
public class DeviceTokenMigrationUTest
{
    private const string DatabaseName = "backend-configuration-pn-migration-tests";

    private const string PreviousMigration =
        "20260724043849_AddDeviceTokensAndAdhocReminderMarkers";

    private const string MigrationUnderTest =
        "20260830081550_DeviceTokenIdentityModel";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    private BackendConfigurationPnDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _dbContext = new BackendConfigurationPnContextFactory()
            .CreateDbContext(new[] { ConnectionString });
        _dbContext.Database.SetCommandTimeout(600);

        // Rebuilt per test: these tests deliberately leave the database
        // half-migrated, so nothing may be inherited from the previous one.
        _dbContext.Database.EnsureDeleted();
        MigrateTo(PreviousMigration);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public void Migration_PopulatedTable_BackfillsEveryRowAndUniqueIndexHolds()
    {
        SeedOldSchemaRows();

        MigrateToLatest();

        var deviceTokenList = _dbContext.DeviceTokens.AsNoTracking().OrderBy(x => x.Id).ToList();

        Assert.That(deviceTokenList, Has.Count.EqualTo(3));
        Assert.That(deviceTokenList.Select(x => x.AppId), Is.All.EqualTo("adhoc"));

        // Derived from the primary key, so the two rows that share an FcmToken
        // - and the row whose FcmToken is NULL - each get a distinct value.
        Assert.That(deviceTokenList.Select(x => x.InstallationId).Distinct().Count(), Is.EqualTo(3));
        Assert.That(deviceTokenList[0].InstallationId, Is.EqualTo("legacy:1"));
        Assert.That(deviceTokenList[1].InstallationId, Is.EqualTo("legacy:2"));
        Assert.That(deviceTokenList[2].InstallationId, Is.EqualTo("legacy:3"));

        // The rename carried the values across.
        Assert.That(deviceTokenList.Select(x => x.SdkSiteId), Is.EqualTo(new[] { 11, 12, 13 }));
        Assert.That(deviceTokenList[0].FcmToken, Is.EqualTo("shared-token"));
        Assert.That(deviceTokenList[1].FcmToken, Is.EqualTo("shared-token"));
        Assert.That(deviceTokenList[2].FcmToken, Is.Null);

        var deviceTokenVersionList =
            _dbContext.DeviceTokenVersions.AsNoTracking().OrderBy(x => x.Id).ToList();

        Assert.That(deviceTokenVersionList, Has.Count.EqualTo(3));
        Assert.That(deviceTokenVersionList.Select(x => x.AppId), Is.All.EqualTo("adhoc"));
        Assert.That(
            deviceTokenVersionList.Select(x => x.InstallationId),
            Is.EqualTo(new[] { "legacy:1", "legacy:2", "legacy:3" }));

        // Two columns, NON_UNIQUE = 0: the identity is enforced, not merely
        // backfilled.
        Assert.That(
            ScalarInt(
                "SELECT COUNT(*) FROM information_schema.STATISTICS " +
                "WHERE table_schema = DATABASE() AND table_name = 'DeviceTokens' " +
                "AND index_name = 'IX_DeviceTokens_AppId_InstallationId' AND NON_UNIQUE = 0"),
            Is.EqualTo(2));

        Assert.That(
            () => { _dbContext.Database.ExecuteSqlRaw(NewRowSql(4, 14, "'another-token'", "'adhoc'", "'legacy:1'")); },
            Throws.Exception,
            "the unique index must reject a second row on (adhoc, legacy:1)");
    }

    [Test]
    public void Migration_RerunAfterHistoryRowLost_IsANoOp()
    {
        SeedOldSchemaRows();
        MigrateToLatest();

        // MariaDB auto-commits every DDL statement, so __EFMigrationsHistory is
        // written only after the last one. A crash anywhere in Up leaves the
        // schema changed with no history row, exactly as this delete does, and
        // the next pod start re-runs Up from the top.
        _dbContext.Database.ExecuteSqlRaw(
            "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '" + MigrationUnderTest + "'");

        Assert.That(() => MigrateToLatest(), Throws.Nothing);

        var deviceTokenList = _dbContext.DeviceTokens.AsNoTracking().OrderBy(x => x.Id).ToList();

        Assert.That(deviceTokenList, Has.Count.EqualTo(3));
        Assert.That(deviceTokenList.Select(x => x.AppId), Is.All.EqualTo("adhoc"));
        Assert.That(
            deviceTokenList.Select(x => x.InstallationId),
            Is.EqualTo(new[] { "legacy:1", "legacy:2", "legacy:3" }));

        Assert.That(
            ScalarInt(
                "SELECT COUNT(*) FROM information_schema.STATISTICS " +
                "WHERE table_schema = DATABASE() AND table_name = 'DeviceTokens' " +
                "AND index_name = 'IX_DeviceTokens_AppId_InstallationId'"),
            Is.EqualTo(2));
    }

    [Test]
    public void Migration_ResumesAfterPartialApplication_AndSweepsRowsWrittenInTheWindow()
    {
        SeedOldSchemaRows();

        // Hand-apply the first half of Up, then stop: the state a pod is left
        // in when it dies mid-migration. The statements mirror the migration's
        // own on purpose - an unguarded re-run dies on the first DROP INDEX
        // with ERROR 1091, and that is what this pins.
        _dbContext.Database.ExecuteSqlRaw(
            "DROP INDEX `IX_DeviceTokens_WorkerId_FcmToken` ON `DeviceTokens`");
        _dbContext.Database.ExecuteSqlRaw(
            "DROP INDEX `IX_DeviceTokens_WorkerId` ON `DeviceTokens`");
        _dbContext.Database.ExecuteSqlRaw(
            "ALTER TABLE `DeviceTokens` CHANGE COLUMN `WorkerId` `SdkSiteId` int NOT NULL");
        _dbContext.Database.ExecuteSqlRaw(
            "ALTER TABLE `DeviceTokenVersions` CHANGE COLUMN `WorkerId` `SdkSiteId` int NOT NULL");
        _dbContext.Database.ExecuteSqlRaw(
            "ALTER TABLE `DeviceTokens` ADD COLUMN `AppId` varchar(32) CHARACTER SET utf8mb4 NULL DEFAULT 'adhoc'");
        _dbContext.Database.ExecuteSqlRaw(
            "ALTER TABLE `DeviceTokens` ADD COLUMN `InstallationId` varchar(128) CHARACTER SET utf8mb4 NULL");

        // A rolling deploy keeps old pods serving while the new pod migrates.
        // This is one of their registrations landing in the window: it names no
        // AppId (the default covers that) and no InstallationId (nothing can).
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `DeviceTokens` " +
            "(`Id`, `SdkSiteId`, `FcmToken`, `Platform`, `CreatedAt`, `UpdatedAt`, " +
            "`WorkflowState`, `CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES " +
            "(4, 14, 'window-token', 'android', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)");

        Assert.That(() => MigrateToLatest(), Throws.Nothing);

        var deviceTokenList = _dbContext.DeviceTokens.AsNoTracking().OrderBy(x => x.Id).ToList();

        Assert.That(deviceTokenList, Has.Count.EqualTo(4));
        Assert.That(deviceTokenList.Select(x => x.AppId), Is.All.EqualTo("adhoc"));
        Assert.That(
            deviceTokenList.Select(x => x.InstallationId),
            Is.EqualTo(new[] { "legacy:1", "legacy:2", "legacy:3", "legacy:4" }),
            "the row written during the window must be swept, not left NULL");
    }

    [Test]
    public void Down_WithoutDuplicates_RollsBack()
    {
        SeedOldSchemaRows();
        MigrateToLatest();

        Assert.That(() => MigrateTo(PreviousMigration), Throws.Nothing);

        Assert.That(ColumnCount("DeviceTokens", "WorkerId"), Is.EqualTo(1));
        Assert.That(ColumnCount("DeviceTokens", "AppId"), Is.EqualTo(0));
    }

    [Test]
    public void Down_WithDuplicateSiteAndToken_AbortsBeforeDroppingAnything()
    {
        SeedOldSchemaRows();
        MigrateToLatest();

        // Legal under the new model, fatal to the old unique key: two installs
        // on one site sharing a token.
        _dbContext.Database.ExecuteSqlRaw(
            NewRowSql(4, 11, "'shared-token'", "'adhoc'", "'install-dup'"));

        Assert.That(
            () => MigrateTo(PreviousMigration),
            Throws.Exception.Message.Contains("Rollback aborted"));

        // The point of the guard: it refuses while the data is still there.
        Assert.That(ColumnCount("DeviceTokens", "AppId"), Is.EqualTo(1));
        Assert.That(ColumnCount("DeviceTokens", "InstallationId"), Is.EqualTo(1));
        Assert.That(ColumnCount("DeviceTokenVersions", "AppId"), Is.EqualTo(1));
        Assert.That(_dbContext.DeviceTokens.AsNoTracking().Count(), Is.EqualTo(4));
    }

    private void MigrateTo(string targetMigration) =>
        _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    private void MigrateToLatest() =>
        _dbContext.GetService<IMigrator>().Migrate();

    // Rows 1 and 2 share an FcmToken across two workers - legal under the old
    // (WorkerId, FcmToken) key, and the shape that made hashing the token an
    // ERROR 1062 in the first place. Row 3 has none at all: SHA2(NULL) is NULL,
    // which would have failed the NOT NULL tightening.
    private void SeedOldSchemaRows()
    {
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `DeviceTokens` " +
            "(`Id`, `WorkerId`, `FcmToken`, `Platform`, `CreatedAt`, `UpdatedAt`, " +
            "`WorkflowState`, `CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES " +
            "(1, 11, 'shared-token', 'android', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1), " +
            "(2, 12, 'shared-token', 'ios', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1), " +
            "(3, 13, NULL, 'android', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)");

        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `DeviceTokenVersions` " +
            "(`Id`, `DeviceTokenId`, `WorkerId`, `FcmToken`, `Platform`, `CreatedAt`, `UpdatedAt`, " +
            "`WorkflowState`, `CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES " +
            "(1, 1, 11, 'shared-token', 'android', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1), " +
            "(2, 2, 12, 'shared-token', 'ios', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1), " +
            "(3, 3, 13, NULL, 'android', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)");
    }

    private static string NewRowSql(int id, int sdkSiteId, string fcmToken, string appId, string installationId) =>
        "INSERT INTO `DeviceTokens` " +
        "(`Id`, `AppId`, `InstallationId`, `SdkSiteId`, `FcmToken`, `Platform`, `CreatedAt`, " +
        "`UpdatedAt`, `WorkflowState`, `CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES (" +
        id + ", " + appId + ", " + installationId + ", " + sdkSiteId + ", " + fcmToken +
        ", 'android', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)";

    private int ColumnCount(string table, string column) =>
        ScalarInt(
            "SELECT COUNT(*) FROM information_schema.COLUMNS " +
            "WHERE table_schema = DATABASE() AND table_name = '" + table + "' " +
            "AND column_name = '" + column + "'");

    private int ScalarInt(string sql)
    {
        var connection = _dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt32(command.ExecuteScalar());
    }
}
