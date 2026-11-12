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
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

// #1335: AddDeviceTokenInstallReport must survive a re-run after a partial
// application (MariaDB DDL is not transactional). Own database: the tests roll
// the schema backwards, which the shared one cannot survive.
[TestFixture]
public class DeviceTokenInstallReportMigrationUTest
{
    private const string DatabaseName = "backend-configuration-pn-install-report-migration-tests";

    private const string PreviousMigration = "20261030062544_AddLederAssignmentSiteId";

    private const string MigrationUnderTest = "20261112092917_AddDeviceTokenInstallReport";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    private static readonly string[] NewColumns =
        ["AppVersion", "AppBuild", "DeviceModel", "DeviceManufacturer", "OsVersion", "LastSeenAt"];

    private BackendConfigurationPnDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _dbContext = new BackendConfigurationPnContextFactory()
            .CreateDbContext(new[] { ConnectionString });
        _dbContext.Database.SetCommandTimeout(600);
        _dbContext.Database.EnsureDeleted();
        MigrateTo(PreviousMigration);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var dbContext = new BackendConfigurationPnContextFactory()
            .CreateDbContext(new[] { ConnectionString });
        dbContext.Database.SetCommandTimeout(600);
        dbContext.Database.EnsureDeleted();
    }

    [Test]
    public void Migration_AddsEveryColumnToBothTables()
    {
        MigrateTo(MigrationUnderTest);

        AssertColumns(expected: 1);
    }

    // The SQL is hand-written (guarded), so pin that it creates what the model
    // maps: the same type and length EF would have generated.
    [TestCase("DeviceTokens", "AppVersion", "varchar(32)")]
    [TestCase("DeviceTokens", "AppBuild", "varchar(32)")]
    [TestCase("DeviceTokens", "DeviceModel", "varchar(128)")]
    [TestCase("DeviceTokens", "DeviceManufacturer", "varchar(128)")]
    [TestCase("DeviceTokens", "OsVersion", "varchar(64)")]
    [TestCase("DeviceTokens", "LastSeenAt", "datetime(6)")]
    [TestCase("DeviceTokenVersions", "AppVersion", "longtext")]
    [TestCase("DeviceTokenVersions", "LastSeenAt", "datetime(6)")]
    public void Migration_CreatesTheTypeTheModelMaps(string table, string column, string expectedType)
    {
        MigrateTo(MigrationUnderTest);

        Assert.That(Scalar(
            "SELECT COLUMN_TYPE FROM information_schema.COLUMNS " +
            "WHERE table_schema = DATABASE() AND table_name = '" + table + "' " +
            "AND column_name = '" + column + "'"), Is.EqualTo(expectedType));
    }

    [Test]
    public void Migration_ResumesAfterPartialApplication()
    {
        // The state a pod is left in when it dies part-way through Up(): the first
        // columns exist, the migration is not recorded. An unguarded re-run would
        // fail with a duplicate column here.
        _dbContext.Database.ExecuteSqlRaw(
            "ALTER TABLE `DeviceTokens` ADD COLUMN `AppVersion` varchar(32) CHARACTER SET utf8mb4 NULL");
        _dbContext.Database.ExecuteSqlRaw(
            "ALTER TABLE `DeviceTokens` ADD COLUMN `AppBuild` varchar(32) CHARACTER SET utf8mb4 NULL");

        Assert.That(() => MigrateTo(MigrationUnderTest), Throws.Nothing);

        AssertColumns(expected: 1);
    }

    [Test]
    public void Down_RemovesEveryColumnFromBothTables()
    {
        MigrateTo(MigrationUnderTest);

        MigrateTo(PreviousMigration);

        AssertColumns(expected: 0);
    }

    private void AssertColumns(int expected)
    {
        Assert.Multiple(() =>
        {
            foreach (var column in NewColumns)
            {
                Assert.That(ColumnCount("DeviceTokens", column), Is.EqualTo(expected), $"DeviceTokens.{column}");
                Assert.That(ColumnCount("DeviceTokenVersions", column), Is.EqualTo(expected), $"DeviceTokenVersions.{column}");
            }
        });
    }

    // Always an explicit target, never Migrate() with no argument, so a later
    // migration cannot quietly take over these tests.
    private void MigrateTo(string targetMigration) =>
        _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    private int ColumnCount(string table, string column) =>
        Convert.ToInt32(Scalar(
            "SELECT COUNT(*) FROM information_schema.COLUMNS " +
            "WHERE table_schema = DATABASE() AND table_name = '" + table + "' " +
            "AND column_name = '" + column + "'"));

    private object Scalar(string sql)
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }
}
