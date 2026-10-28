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
using MySqlConnector;
using NUnit.Framework;

// Own database: the test rolls the schema back to the migration before
// AddLederAssignments, which the shared test database cannot survive.
[TestFixture]
public class LederAssignmentMigrationUTest
{
    private const string DatabaseName = "backend-configuration-pn-leder-migration-tests";

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
        _migrationUnderTest = all.Single(m => m.EndsWith("_AddLederAssignments", StringComparison.Ordinal));
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
    public void Migration_CreatesTablesAndUniqueIndex()
    {
        Assert.That(TableCount("LederAssignments"), Is.EqualTo(0), "absent before the migration");
        Assert.That(TableCount("LederAssignmentVersions"), Is.EqualTo(0), "absent before the migration");

        MigrateTo(_migrationUnderTest);

        Assert.That(TableCount("LederAssignments"), Is.EqualTo(1));
        Assert.That(TableCount("LederAssignmentVersions"), Is.EqualTo(1));

        Assert.That(
            UniqueIndexColumnCount("LederAssignments", "IX_LederAssignments_EformUserId"),
            Is.EqualTo(1));
    }

    [Test]
    public void Migration_UniqueIndexRefusesSecondRow_AlsoWhenFirstIsRemoved()
    {
        MigrateTo(_migrationUnderTest);

        _dbContext.Database.ExecuteSqlRaw(InsertSql(1, 601, "created"));

        Assert.That(
            () => _dbContext.Database.ExecuteSqlRaw(InsertSql(2, 601, "created")),
            Throws.InstanceOf<MySqlException>().With.Property(nameof(MySqlException.Number)).EqualTo(DuplicateEntry),
            "a second live row for the same login must be refused");

        _dbContext.Database.ExecuteSqlRaw("UPDATE `LederAssignments` SET `WorkflowState` = 'removed' WHERE `Id` = 1");

        Assert.That(
            () => _dbContext.Database.ExecuteSqlRaw(InsertSql(3, 601, "created")),
            Throws.InstanceOf<MySqlException>().With.Property(nameof(MySqlException.Number)).EqualTo(DuplicateEntry),
            "the index has no WorkflowState filter: a removed row still blocks a second row");

        _dbContext.Database.ExecuteSqlRaw(InsertSql(4, 602, "created"));
        Assert.That(ScalarInt("SELECT COUNT(*) FROM `LederAssignments`"), Is.EqualTo(2));
    }

    private static BackendConfigurationPnDbContext CreateContext() =>
        new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });

    private void MigrateTo(string targetMigration) =>
        _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    // Values are literals owned by this test; ids are explicit so nothing assumes an auto-increment value.
    private static string InsertSql(int id, int eformUserId, string workflowState) =>
        "INSERT INTO `LederAssignments` " +
        "(`Id`, `EformUserId`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, `CreatedByUserId`, " +
        "`UpdatedByUserId`, `Version`) VALUES (" +
        id + ", " + eformUserId + ", UTC_TIMESTAMP(), UTC_TIMESTAMP(), '" + workflowState + "', 1, 1, 1)";

    private int TableCount(string table) =>
        ScalarInt(
            "SELECT COUNT(*) FROM information_schema.TABLES " +
            "WHERE table_schema = DATABASE() AND table_name = '" + table + "'");

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
