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
using System.Data;
using System.Linq;
using Infrastructure.Data;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;

// Migrates to the migration before ChemicalInventory, writes legacy rows at
// the old schema, then migrates forward: the legacy tables and Property
// columns must be gone, the property itself must survive, and the new tables
// must exist with their unique identities enforced.
[TestFixture]
public class ChemicalInventoryMigrationUTest
{
    private const string DatabaseName = "backend-configuration-pn-chemical-migration-tests";
    private const string PreviousMigration = "20260830081550_DeviceTokenIdentityModel";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    private static readonly string[] LegacyTables =
    [
        "ChemicalProductProperties", "ChemicalProductPropertyVersions",
        "ChemicalProductPropertieSites", "ChemicalProductPropertyVersionSites"
    ];

    private static readonly string[] LegacyPropertyColumns =
    [
        "EntitySearchListChemicals", "EntitySearchListChemicalRegNos",
        "EntitySelectListChemicalAreas", "ChemicalLastUpdatedAt"
    ];

    private static readonly string[] NewTables =
    [
        "ChemicalLocations", "ChemicalLocationVersions", "ChemicalPlacements", "ChemicalPlacementVersions",
        "ChemicalStockEntries", "ChemicalStockEntryVersions", "ChemicalPropertySettings",
        "ChemicalPropertySettingsVersions", "ChemicalWorkerPermissions", "ChemicalWorkerPermissionVersions",
        "ChemicalAlertLogs", "ChemicalAlertLogVersions"
    ];

    private BackendConfigurationPnDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _dbContext = new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });
        _dbContext.Database.SetCommandTimeout(600);
        _dbContext.Database.EnsureDeleted();
        MigrateTo(PreviousMigration);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var dbContext = new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });
        dbContext.Database.SetCommandTimeout(600);
        dbContext.Database.EnsureDeleted();
    }

    [Test]
    public void Migration_DropsLegacyChemicalSchema_KeepsProperties_CreatesInventoryTables()
    {
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `Properties` (`Id`, `Name`, `ItemPlanningTagId`, `ComplianceStatus`, `ComplianceStatusThirty`, " +
            "`WorkorderEnable`, `IsFarm`, `EntitySearchListChemicals`, `EntitySearchListChemicalRegNos`, " +
            "`EntitySelectListChemicalAreas`, `ChemicalLastUpdatedAt`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
            "`CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES " +
            "(1, 'Gården', 0, 0, 0, 0, 1, 11, 12, 13, UTC_TIMESTAMP(), UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)");
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `ChemicalProductProperties` (`Id`, `ChemicalId`, `ProductId`, `PropertyId`, `SdkCaseId`, " +
            "`Locations`, `LanguageId`, `SdkSiteId`, `ExpireDate`, `LastFolderName`, `CreatedAt`, `UpdatedAt`, " +
            "`WorkflowState`, `CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES " +
            "(1, 5, 6, 1, 7, 'Lade', 1, 8, UTC_TIMESTAMP(), '25.05', UTC_TIMESTAMP(), UTC_TIMESTAMP(), 'created', 1, 1, 1)");

        MigrateTo(MigrationUnderTest());

        foreach (var table in LegacyTables)
        {
            Assert.That(TableCount(table), Is.EqualTo(0), table);
        }

        foreach (var column in LegacyPropertyColumns)
        {
            Assert.That(ColumnCount("Properties", column), Is.EqualTo(0), column);
            Assert.That(ColumnCount("PropertieVersions", column), Is.EqualTo(0), column);
        }

        foreach (var table in NewTables)
        {
            Assert.That(TableCount(table), Is.EqualTo(1), table);
        }

        Assert.That(_dbContext.Properties.AsNoTracking().Single(x => x.Id == 1).Name, Is.EqualTo("Gården"));
        Assert.That(UniqueIndexColumnCount("ChemicalWorkerPermissions", "IX_ChemicalWorkerPermissions_PropertyId_WorkerId"),
            Is.EqualTo(2));
        Assert.That(UniqueIndexColumnCount("ChemicalPropertySettings", "IX_ChemicalPropertySettings_PropertyId"),
            Is.EqualTo(1));
        Assert.That(DecimalScale("ChemicalStockEntries", "Amount"), Is.EqualTo(3));
    }

    // Resolved by suffix so the generated timestamp never has to be copied by hand.
    private string MigrationUnderTest() =>
        _dbContext.Database.GetMigrations().Single(m => m.EndsWith("_ChemicalInventory", StringComparison.Ordinal));

    private void MigrateTo(string targetMigration) => _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    private int TableCount(string table) => ScalarInt(
        "SELECT COUNT(*) FROM information_schema.TABLES WHERE table_schema = DATABASE() AND table_name = '" + table + "'");

    private int ColumnCount(string table, string column) => ScalarInt(
        "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE table_schema = DATABASE() AND table_name = '" +
        table + "' AND column_name = '" + column + "'");

    private int UniqueIndexColumnCount(string table, string indexName) => ScalarInt(
        "SELECT COUNT(*) FROM information_schema.STATISTICS WHERE table_schema = DATABASE() AND table_name = '" +
        table + "' AND index_name = '" + indexName + "' AND NON_UNIQUE = 0");

    private int DecimalScale(string table, string column) => ScalarInt(
        "SELECT NUMERIC_SCALE FROM information_schema.COLUMNS WHERE table_schema = DATABASE() AND table_name = '" +
        table + "' AND column_name = '" + column + "'");

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
