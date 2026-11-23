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
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

/// <summary>
/// Fixture for the tests of one migration, on its own database (the tests roll the schema backwards,
/// which the shared one cannot survive). One full migration per fixture; each test then starts at the
/// migration before the one under test, and moving across that migration alone is cheap.
/// </summary>
public abstract class MigrationTestBase
{
    protected BackendConfigurationPnDbContext _dbContext;
    protected string _migrationUnderTest;
    protected string _previousMigration;

    /// <summary>The fixture's own database name.</summary>
    protected abstract string DatabaseName { get; }

    /// <summary>The migration under test, by name without its timestamp (e.g. "AddTaskChangeFeed").</summary>
    protected abstract string MigrationName { get; }

    private string ConnectionString =>
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        using var dbContext = CreateContext();
        dbContext.Database.EnsureDeleted();
        var all = dbContext.Database.GetMigrations().ToList();
        _migrationUnderTest = all.Single(m => m.EndsWith("_" + MigrationName, StringComparison.Ordinal));
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

    protected BackendConfigurationPnDbContext CreateContext()
    {
        var dbContext = new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });
        dbContext.Database.SetCommandTimeout(600);
        return dbContext;
    }

    // Always an explicit target, never Migrate() with no argument, so a later migration cannot
    // quietly take over these tests.
    protected void MigrateTo(string targetMigration) =>
        _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    // One row per table, column, index column and foreign key: everything an ALTER of an existing
    // table would change.
    protected List<string> SchemaFingerprint() => Strings(
        "SELECT CONCAT_WS('|', TABLE_NAME, 'col', COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, " +
        "COALESCE(COLUMN_DEFAULT, '<none>'), EXTRA, COALESCE(CHARACTER_SET_NAME, ''), ORDINAL_POSITION) " +
        "FROM information_schema.COLUMNS WHERE table_schema = DATABASE() " +
        "UNION ALL " +
        "SELECT CONCAT_WS('|', TABLE_NAME, 'idx', INDEX_NAME, NON_UNIQUE, SEQ_IN_INDEX, COLUMN_NAME) " +
        "FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
        "UNION ALL " +
        "SELECT CONCAT_WS('|', TABLE_NAME, 'fk', CONSTRAINT_NAME, REFERENCED_TABLE_NAME, DELETE_RULE) " +
        "FROM information_schema.REFERENTIAL_CONSTRAINTS WHERE constraint_schema = DATABASE()");

    protected object Scalar(string sql)
    {
        using var command = OpenCommand(sql);
        return command.ExecuteScalar();
    }

    protected List<string> Strings(string sql)
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
