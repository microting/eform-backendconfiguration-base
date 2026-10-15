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
using Infrastructure.Data.Factories;
using Infrastructure.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MySqlConnector;
using NUnit.Framework;

// The raw SQL below concatenates only literals, table/column names and migration ids.
// Identifiers cannot be parameterised.
#pragma warning disable EF1003

// DbTestFixture migrates an empty database, so the data step in AddArchiveFolders would
// match no rows there. These tests migrate to the migration BEFORE AddArchiveFolders,
// write Files/FileVersions rows at the old schema, and only then migrate forward.
// The tests roll the schema back, which the shared test database cannot survive, so this
// fixture uses its own database.
[TestFixture]
public class ArchiveFoldersMigrationUTest
{
    private const string DatabaseName = "backend-configuration-pn-archive-migration-tests";

    private const string ConnectionString =
        "Server = localhost; port = 3306; Database = " + DatabaseName +
        "; user = root; password = secretpassword; Convert Zero Datetime = true;";

    // MySQL/MariaDB error 1452: "Cannot add or update a child row: a foreign key constraint fails".
    private const int ForeignKeyViolation = 1452;

    private BackendConfigurationPnDbContext _dbContext;
    private string _migrationUnderTest;
    private string _previousMigration;

    [SetUp]
    public void Setup()
    {
        _dbContext = CreateContext();
        _dbContext.Database.EnsureDeleted();

        // Resolved by suffix and position, so nobody has to copy the generated timestamps by hand.
        var all = _dbContext.Database.GetMigrations().ToList();
        _migrationUnderTest = all.Single(m => m.EndsWith("_AddArchiveFolders", StringComparison.Ordinal));
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
    public void Migration_MovesEveryFileAndVersionIntoArkiv()
    {
        InsertOldFile(1, "Faktura 2025-001.pdf", "created");
        InsertOldFile(2, "Servicerapport.pdf", "created");
        InsertOldFile(3, "Gammel kontrakt.pdf", "removed");
        InsertOldFileVersion(1, 1);
        InsertOldFileVersion(2, 3);

        MigrateTo(_migrationUnderTest);

        var folder = _dbContext.ArchiveFolders.AsNoTracking().Single();
        Assert.That(folder.Name, Is.EqualTo("Arkiv"));
        Assert.That(folder.IsDefault, Is.True);
        Assert.That(folder.ParentId, Is.Null);
        Assert.That(folder.WorkflowState, Is.EqualTo("created"));
        Assert.That(folder.Version, Is.EqualTo(1));
        Assert.That(folder.CreatedByUserId, Is.EqualTo(0));
        Assert.That(folder.UpdatedByUserId, Is.EqualTo(0));
        Assert.That(folder.UpdatedAt, Is.Not.Null);

        var folderVersion = _dbContext.ArchiveFolderVersions.AsNoTracking().Single();
        Assert.That(folderVersion.ArchiveFolderId, Is.EqualTo(folder.Id));
        Assert.That(folderVersion.Name, Is.EqualTo("Arkiv"));
        Assert.That(folderVersion.IsDefault, Is.True);
        Assert.That(folderVersion.Version, Is.EqualTo(1));

        var files = _dbContext.Files.AsNoTracking().OrderBy(f => f.Id).ToList();
        Assert.That(files, Has.Count.EqualTo(3));
        Assert.That(files.Select(f => f.FolderId), Is.All.EqualTo(folder.Id));
        Assert.That(files.Select(f => f.DocumentDate), Is.All.Null);
        Assert.That(files.Select(f => f.DocumentDateState), Is.All.EqualTo(DocumentDateState.None));
        Assert.That(files.Select(f => f.OwnAccess), Is.All.False);
        Assert.That(files[2].WorkflowState, Is.EqualTo("removed"));

        var versions = _dbContext.FileVersions.AsNoTracking().ToList();
        Assert.That(versions, Has.Count.EqualTo(2));
        Assert.That(versions.Select(v => v.FolderId), Is.All.EqualTo(folder.Id));
    }

    [Test]
    public void Migration_TightensFolderId_AddsForeignKeyAndIndexes()
    {
        InsertOldFile(1, "Faktura.pdf", "created");

        MigrateTo(_migrationUnderTest);

        Assert.That(IsNullable("Files", "FolderId"), Is.EqualTo("NO"));
        Assert.That(IsNullable("FileVersions", "FolderId"), Is.EqualTo("YES"));
        Assert.That(ForeignKeyCount("Files", "FolderId", "ArchiveFolders"), Is.EqualTo(1));
        Assert.That(ForeignKeyCount("FileVersions", "FolderId", "ArchiveFolders"), Is.EqualTo(0));
        Assert.That(ForeignKeyCount("ArchiveFolders", "ParentId", "ArchiveFolders"), Is.EqualTo(1));
        Assert.That(ForeignKeyCount("ArchiveFolderReadTags", "ArchiveFolderId", "ArchiveFolders"), Is.EqualTo(1));
        Assert.That(ForeignKeyCount("FileReadTags", "FileId", "Files"), Is.EqualTo(1));

        Assert.That(IndexCount("Files", "IX_Files_FolderId"), Is.EqualTo(1));
        Assert.That(IndexCount("ArchiveFolders", "IX_ArchiveFolders_ParentId"), Is.EqualTo(1));
        Assert.That(IndexCount("ArchiveFolderReadTags", "IX_ArchiveFolderReadTags_ArchiveFolderId"), Is.EqualTo(1));
        Assert.That(IndexCount("ArchiveFolderReadTags", "IX_ArchiveFolderReadTags_SdkTagId"), Is.EqualTo(1));
        Assert.That(IndexCount("FileReadTags", "IX_FileReadTags_FileId"), Is.EqualTo(1));
        Assert.That(IndexCount("FileReadTags", "IX_FileReadTags_SdkTagId"), Is.EqualTo(1));

        Assert.That(ColumnType("Files", "DocumentDate"), Is.EqualTo("date"));
        Assert.That(ColumnType("FileVersions", "DocumentDate"), Is.EqualTo("date"));
        Assert.That(ColumnType("InboxSuggestions", "TargetDate"), Is.EqualTo("date"));
        Assert.That(ColumnType("InboxSuggestionVersions", "TargetDate"), Is.EqualTo("date"));
        Assert.That(IsNullable("Files", "DocumentDateState"), Is.EqualTo("NO"));
        Assert.That(IsNullable("Files", "OwnAccess"), Is.EqualTo("NO"));

        // ExecuteSqlRaw does not wrap provider errors (only SaveChanges wraps them in
        // DbUpdateException), and the retrying strategy rethrows a non-transient error unchanged.
        Assert.That(
            () => _dbContext.Database.ExecuteSqlRaw(
                "INSERT INTO `Files` (`FileName`, `FolderId`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
                "`CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES " +
                "('Forkert mappe.pdf', 999999, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 'created', 0, 0, 1)"),
            Throws.InstanceOf<MySqlException>().With.Property(nameof(MySqlException.Number))
                .EqualTo(ForeignKeyViolation),
            "the FK must reject a folder id that does not exist");
    }

    [Test]
    public void Migration_MovesFilesBeyondOneBatchWithIdGaps()
    {
        // 12,345 files and 12,345 file versions, both with ids 2, 4, ... 24,690: five 5,000-id
        // ranges, the last one partial, and every second id missing. seq_1_to_N is MariaDB's
        // built-in Sequence engine.
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `Files` (`Id`, `FileName`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
            "`CreatedByUserId`, `UpdatedByUserId`, `Version`) " +
            "SELECT seq * 2, CONCAT('Dokument ', seq, '.pdf'), UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), " +
            "'created', 0, 0, 1 FROM seq_1_to_12345");
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `FileVersions` (`Id`, `FileId`, `FileName`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
            "`CreatedByUserId`, `UpdatedByUserId`, `Version`) " +
            "SELECT seq * 2, seq * 2, CONCAT('Dokument ', seq, '.pdf'), UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), " +
            "'created', 0, 0, 1 FROM seq_1_to_12345");

        MigrateTo(_migrationUnderTest);

        var folderId = DefaultFolderId();
        Assert.That(ScalarInt("SELECT COUNT(*) FROM `Files` WHERE `FolderId` = " + folderId), Is.EqualTo(12345));
        Assert.That(ScalarInt("SELECT COUNT(*) FROM `FileVersions`"), Is.EqualTo(12345));
        Assert.That(ScalarInt("SELECT COUNT(*) FROM `FileVersions` WHERE `FolderId` = " + folderId), Is.EqualTo(12345));
    }

    [Test]
    public void Migration_EmptyTenant_GetsArkiv()
    {
        MigrateTo(_migrationUnderTest);

        var folder = _dbContext.ArchiveFolders.AsNoTracking().Single();
        Assert.That(folder.Name, Is.EqualTo("Arkiv"));
        Assert.That(folder.IsDefault, Is.True);
        Assert.That(_dbContext.ArchiveFolderVersions.AsNoTracking().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Migration_RerunAfterHistoryRowLost_IsANoOp()
    {
        InsertOldFile(1, "Faktura.pdf", "created");
        MigrateTo(_migrationUnderTest);

        // MariaDB commits every DDL statement on its own, so __EFMigrationsHistory is written
        // only after the last one. If the migration crashes anywhere in Up, the schema is changed
        // but no history row exists, which is exactly what this delete leaves behind.
        DeleteHistoryRow();

        Assert.That(() => MigrateTo(_migrationUnderTest), Throws.Nothing);

        Assert.That(_dbContext.ArchiveFolders.AsNoTracking().Count(), Is.EqualTo(1));
        Assert.That(_dbContext.ArchiveFolderVersions.AsNoTracking().Count(), Is.EqualTo(1));
        Assert.That(_dbContext.Files.AsNoTracking().Single().FolderId, Is.EqualTo(DefaultFolderId()));
        Assert.That(ForeignKeyCount("Files", "FolderId", "ArchiveFolders"), Is.EqualTo(1));
    }

    [Test]
    public void Migration_ResumesFromPartialState_AndMovesLateRows()
    {
        InsertOldFile(1, "Faktura.pdf", "created");
        MigrateTo(_migrationUnderTest);
        RewindToPartialState();

        // An old pod (it does not know FolderId) writes a file in that window.
        InsertOldFile(2, "Uploadet under opgradering.pdf", "created");

        Assert.That(() => MigrateTo(_migrationUnderTest), Throws.Nothing);

        // The re-run's batched move (step 4) picks up the late row, because it was written
        // before the re-run read MAX(Id). The final sweep (step 6) exists for a row written
        // after that read and before the MODIFY. That window needs concurrency to hit, so it is
        // covered by reasoning in the migration, not by this test.
        var folderId = DefaultFolderId();
        Assert.That(_dbContext.Files.AsNoTracking().Select(f => f.FolderId).ToList(),
            Is.All.EqualTo(folderId));
        Assert.That(_dbContext.ArchiveFolderVersions.AsNoTracking().Single().ArchiveFolderId,
            Is.EqualTo(folderId), "a lost version row is restored, without a second folder");
        Assert.That(IsNullable("Files", "FolderId"), Is.EqualTo("NO"));
        Assert.That(ForeignKeyCount("Files", "FolderId", "ArchiveFolders"), Is.EqualTo(1));
    }

    [Test]
    public void Migration_ResumesFromPartialState_MovesZeroFolderIds()
    {
        // A server that is not in strict mode turns NULL into 0 when an earlier tightening
        // failed. The move treats 0 like NULL.
        InsertOldFile(1, "Faktura.pdf", "created");
        InsertOldFileVersion(1, 1);
        MigrateTo(_migrationUnderTest);
        RewindToPartialState();
        _dbContext.Database.ExecuteSqlRaw("UPDATE `Files` SET `FolderId` = 0");
        _dbContext.Database.ExecuteSqlRaw("UPDATE `FileVersions` SET `FolderId` = 0");

        Assert.That(() => MigrateTo(_migrationUnderTest), Throws.Nothing);

        var folderId = DefaultFolderId();
        Assert.That(_dbContext.Files.AsNoTracking().Single().FolderId, Is.EqualTo(folderId));
        Assert.That(_dbContext.FileVersions.AsNoTracking().Single().FolderId, Is.EqualTo(folderId));
        Assert.That(ForeignKeyCount("Files", "FolderId", "ArchiveFolders"), Is.EqualTo(1));
    }

    [Test]
    public void Migration_Down_RemovesSchemaAndKeepsFiles()
    {
        InsertOldFile(1, "Faktura.pdf", "created");
        MigrateTo(_migrationUnderTest);

        MigrateTo(_previousMigration);

        foreach (var table in new[]
                 {
                     "ArchiveFolders", "ArchiveFolderVersions", "ArchiveFolderReadTags",
                     "ArchiveFolderReadTagVersions", "FileReadTags", "FileReadTagVersions"
                 })
        {
            Assert.That(TableCount(table), Is.EqualTo(0), table);
        }

        foreach (var column in new[] { "FolderId", "DocumentDate", "DocumentDateState", "OwnAccess" })
        {
            Assert.That(ColumnCount("Files", column), Is.EqualTo(0), column);
            Assert.That(ColumnCount("FileVersions", column), Is.EqualTo(0), column);
        }

        Assert.That(ColumnCount("InboxSuggestions", "TargetDate"), Is.EqualTo(0));
        Assert.That(ColumnCount("InboxSuggestionVersions", "TargetDate"), Is.EqualTo(0));
        Assert.That(ScalarInt("SELECT COUNT(*) FROM `Files`"), Is.EqualTo(1));
    }

    private static BackendConfigurationPnDbContext CreateContext()
    {
        var dbContext = new BackendConfigurationPnContextFactory().CreateDbContext(new[] { ConnectionString });
        dbContext.Database.SetCommandTimeout(600);
        return dbContext;
    }

    private int DefaultFolderId() => _dbContext.ArchiveFolders.AsNoTracking().Single().Id;

    // Puts the database back in the state it has between the data step and the tightening,
    // with a lost folder version row, and without the history row.
    private void RewindToPartialState()
    {
        DeleteHistoryRow();
        _dbContext.Database.ExecuteSqlRaw(
            "ALTER TABLE `Files` DROP FOREIGN KEY `FK_Files_ArchiveFolders_FolderId`");
        _dbContext.Database.ExecuteSqlRaw("ALTER TABLE `Files` MODIFY COLUMN `FolderId` int NULL");
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM `ArchiveFolderVersions`");
    }

    private void InsertOldFile(int id, string fileName, string workflowState) =>
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `Files` (`Id`, `FileName`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
            "`CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES (" + id + ", '" + fileName +
            "', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), '" + workflowState + "', 0, 0, 1)");

    private void InsertOldFileVersion(int id, int fileId) =>
        _dbContext.Database.ExecuteSqlRaw(
            "INSERT INTO `FileVersions` (`Id`, `FileId`, `FileName`, `CreatedAt`, `UpdatedAt`, `WorkflowState`, " +
            "`CreatedByUserId`, `UpdatedByUserId`, `Version`) VALUES (" + id + ", " + fileId +
            ", 'v.pdf', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 'created', 0, 0, 1)");

    private void DeleteHistoryRow() =>
        _dbContext.Database.ExecuteSqlRaw(
            "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '" + _migrationUnderTest + "'");

    private void MigrateTo(string targetMigration) => _dbContext.GetService<IMigrator>().Migrate(targetMigration);

    private int TableCount(string table) => ScalarInt(
        "SELECT COUNT(*) FROM information_schema.TABLES WHERE table_schema = DATABASE() AND table_name = '" +
        table + "'");

    private int ColumnCount(string table, string column) =>
        ScalarInt("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE " + ColumnFilter(table, column));

    private string IsNullable(string table, string column) => ColumnAttribute("IS_NULLABLE", table, column);

    private string ColumnType(string table, string column) => ColumnAttribute("DATA_TYPE", table, column);

    private string ColumnAttribute(string attribute, string table, string column) =>
        ScalarString("SELECT " + attribute + " FROM information_schema.COLUMNS WHERE " + ColumnFilter(table, column));

    private static string ColumnFilter(string table, string column) =>
        "table_schema = DATABASE() AND table_name = '" + table + "' AND column_name = '" + column + "'";

    private int IndexCount(string table, string indexName) => ScalarInt(
        "SELECT COUNT(DISTINCT index_name) FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
        "AND table_name = '" + table + "' AND index_name = '" + indexName + "'");

    private int ForeignKeyCount(string table, string column, string referencedTable) => ScalarInt(
        "SELECT COUNT(*) FROM information_schema.KEY_COLUMN_USAGE WHERE table_schema = DATABASE() " +
        "AND table_name = '" + table + "' AND column_name = '" + column +
        "' AND referenced_table_name = '" + referencedTable + "'");

    private int ScalarInt(string sql) => Convert.ToInt32(Scalar(sql));

    private string ScalarString(string sql) => Convert.ToString(Scalar(sql));

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
