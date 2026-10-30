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
using Infrastructure.Data.Entities;
using Infrastructure.Data.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MySqlConnector;
using NUnit.Framework;

// Own database: the test rolls the schema back to the migration before AddLederAssignmentSiteId.
[TestFixture]
public class LederAssignmentSiteIdMigrationUTest
{
    private const string DatabaseName = "backend-configuration-pn-leder-siteid-migration-tests";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    // MySQL/MariaDB error 1062: "Duplicate entry for key".
    private const int DuplicateEntry = 1062;

    private BackendConfigurationPnDbContext _dbContext;
    private string _migrationUnderTest;
    private string _previousMigration;

    [SetUp]
    public void Setup()
    {
        _dbContext = CreateContext();
        _dbContext.Database.EnsureDeleted();

        // Resolved by suffix and position, so nobody copies generated timestamps by hand.
        var all = _dbContext.Database.GetMigrations().ToList();
        _migrationUnderTest = all.Single(m => m.EndsWith("_AddLederAssignmentSiteId", StringComparison.Ordinal));
        _previousMigration = all[all.IndexOf(_migrationUnderTest) - 1];

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
    public void Migration_KeepsLegacyRows_AddsSiteId_AndMakesEformUserIdNullable()
    {
        Assert.That(_previousMigration, Does.EndWith("_AddLederAssignments"), "the migration follows AddLederAssignments");
        _dbContext.Database.ExecuteSqlRaw(LegacyInsertSql(1, 801));

        MigrateTo(_migrationUnderTest);

        Assert.That(
            ScalarInt("SELECT COUNT(*) FROM `LederAssignments` WHERE `Id` = 1 AND `EformUserId` = 801 AND `SiteId` IS NULL"),
            Is.EqualTo(1), "the legacy row survives with SiteId NULL");
        foreach (var table in new[] { "LederAssignments", "LederAssignmentVersions" })
        {
            Assert.That(IsNullable(table, "SiteId"), Is.True, $"{table}.SiteId");
            Assert.That(IsNullable(table, "EformUserId"), Is.True, $"{table}.EformUserId");
        }

        Assert.That(UniqueIndexColumnCount("LederAssignments", "IX_LederAssignments_SiteId"), Is.EqualTo(1));
        Assert.That(UniqueIndexColumnCount("LederAssignments", "IX_LederAssignments_EformUserId"), Is.EqualTo(1),
            "the legacy index stays unique");
    }

    [Test]
    public void Migration_SiteIndexRefusesSecondRow_AlsoWhenFirstIsRemoved_AndAllowsManyNulls()
    {
        MigrateTo(_migrationUnderTest);

        _dbContext.Database.ExecuteSqlRaw(SiteInsertSql(1, 901));
        Assert.That(
            () => _dbContext.Database.ExecuteSqlRaw(SiteInsertSql(2, 901)),
            Throws.InstanceOf<MySqlException>().With.Property(nameof(MySqlException.Number)).EqualTo(DuplicateEntry),
            "a second row for the same site must be refused");

        _dbContext.Database.ExecuteSqlRaw("UPDATE `LederAssignments` SET `WorkflowState` = 'removed' WHERE `Id` = 1");
        Assert.That(
            () => _dbContext.Database.ExecuteSqlRaw(SiteInsertSql(3, 901)),
            Throws.InstanceOf<MySqlException>().With.Property(nameof(MySqlException.Number)).EqualTo(DuplicateEntry),
            "no WorkflowState filter: a removed row still blocks a second row");

        _dbContext.Database.ExecuteSqlRaw(SiteInsertSql(4, 902));
        _dbContext.Database.ExecuteSqlRaw(SiteInsertSql(5, 903));
        Assert.That(ScalarInt("SELECT COUNT(*) FROM `LederAssignments` WHERE `EformUserId` IS NULL"), Is.EqualTo(3),
            "site rows carry no EformUserId and never collide on it");
    }

    [Test]
    public void Model_SiteIdIndex_IsUniqueWithoutAFilter()
    {
        var model = _dbContext.GetService<IDesignTimeModel>().Model;
        var index = model.FindEntityType(typeof(LederAssignment))!.GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(LederAssignment.SiteId) }));

        Assert.That(index.IsUnique, Is.True);
        Assert.That(index.GetFilter(), Is.Null);
    }

    private static BackendConfigurationPnDbContext CreateContext() =>
        new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });

    private void MigrateTo(string targetMigration) =>
        _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    // Literals owned by this test; ids are explicit so nothing assumes an auto-increment value.
    private static string LegacyInsertSql(int id, int eformUserId) =>
        "INSERT INTO `LederAssignments` (`Id`, `EformUserId`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
        "`CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES (" +
        id + ", " + eformUserId + ", UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)";

    private static string SiteInsertSql(int id, int siteId) =>
        "INSERT INTO `LederAssignments` (`Id`, `SiteId`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
        "`CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES (" +
        id + ", " + siteId + ", UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)";

    private bool IsNullable(string table, string column) =>
        ScalarInt(
            "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE table_schema = DATABASE() AND table_name = '" +
            table + "' AND column_name = '" + column + "' AND IS_NULLABLE = 'YES'") == 1;

    // Counts index columns only while the index is actually UNIQUE.
    private int UniqueIndexColumnCount(string table, string indexName) =>
        ScalarInt(
            "SELECT COUNT(*) FROM information_schema.STATISTICS " +
            "WHERE table_schema = DATABASE() AND table_name = '" + table + "' " +
            "AND index_name = '" + indexName + "' AND NON_UNIQUE = 0");

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
