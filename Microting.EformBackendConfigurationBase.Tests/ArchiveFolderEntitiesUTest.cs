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
using System.Linq;
using System.Threading.Tasks;
using eForm.Infrastructure.Constants;
using Infrastructure.Data.Entities;
using Infrastructure.Enum;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NUnit.Framework;

[TestFixture]
public class ArchiveFolderEntitiesUTest : DbTestFixture
{
    // MySQL/MariaDB error 1452: "Cannot add or update a child row: a foreign key constraint fails".
    private const int ForeignKeyViolation = 1452;

    private async Task<ArchiveFolder> CreateFolder(string name, int? parentId = null, bool isDefault = false)
    {
        var folder = new ArchiveFolder
        {
            Name = name,
            ParentId = parentId,
            IsDefault = isDefault,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };
        await folder.Create(DbContext);
        return folder;
    }

    private async Task<File> CreateFile(int folderId, string fileName = "Faktura 2026-001.pdf")
    {
        var file = new File
        {
            FileName = fileName,
            FolderId = folderId,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };
        await file.Create(DbContext);
        return file;
    }

    [Test]
    public void Enums_PersistedValuesNeverShift()
    {
        Assert.Multiple(() =>
        {
            Assert.That((int)DocumentDateState.None, Is.EqualTo(0));
            Assert.That((int)DocumentDateState.Suggested, Is.EqualTo(1));
            Assert.That((int)DocumentDateState.Approved, Is.EqualTo(2));
            Assert.That((int)InboxSuggestionKind.Property, Is.EqualTo(0));
            Assert.That((int)InboxSuggestionKind.Tag, Is.EqualTo(1));
            Assert.That((int)InboxSuggestionKind.Folder, Is.EqualTo(2));
            Assert.That((int)InboxSuggestionKind.DocumentDate, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task ArchiveFolder_Create_SavesRowAndVersion()
    {
        var folder = await CreateFolder("Økonomi og regnskab");

        var rows = await DbContext.ArchiveFolders.AsNoTracking().ToListAsync();
        var versions = await DbContext.ArchiveFolderVersions.AsNoTracking().ToListAsync();
        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].Name, Is.EqualTo("Økonomi og regnskab"));
        Assert.That(rows[0].ParentId, Is.Null);
        Assert.That(rows[0].IsDefault, Is.False);
        Assert.That(rows[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(versions, Has.Count.EqualTo(1));
        Assert.That(versions[0].ArchiveFolderId, Is.EqualTo(folder.Id));
        Assert.That(versions[0].Name, Is.EqualTo("Økonomi og regnskab"));
    }

    [Test]
    public async Task ArchiveFolder_Subfolder_LoadsParentAndChildren()
    {
        var parent = await CreateFolder("Drift");
        await CreateFolder("Fakturaer", parent.Id);
        await CreateFolder("Serviceaftaler", parent.Id);

        var loaded = await DbContext.ArchiveFolders.AsNoTracking()
            .Include(f => f.Children)
            .SingleAsync(f => f.Id == parent.Id);
        Assert.That(loaded.Children.Select(c => c.Name).OrderBy(n => n),
            Is.EqualTo(new[] { "Fakturaer", "Serviceaftaler" }));

        var child = await DbContext.ArchiveFolders.AsNoTracking()
            .Include(f => f.Parent)
            .SingleAsync(f => f.Name == "Fakturaer");
        Assert.That(child.Parent.Name, Is.EqualTo("Drift"));

        var childVersion = await DbContext.ArchiveFolderVersions.AsNoTracking()
            .SingleAsync(v => v.ArchiveFolderId == child.Id);
        Assert.That(childVersion.ParentId, Is.EqualTo(parent.Id));
    }

    [Test]
    public async Task ArchiveFolder_Rename_WritesSecondVersion()
    {
        var folder = await CreateFolder("Kontrakter");

        folder.Name = "Aftaler";
        await folder.Update(DbContext);

        var versions = await DbContext.ArchiveFolderVersions.AsNoTracking()
            .OrderBy(v => v.Version).ToListAsync();
        Assert.That(versions, Has.Count.EqualTo(2));
        Assert.That(versions[1].Name, Is.EqualTo("Aftaler"));
        Assert.That(versions[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task ArchiveFolder_NameOf250Characters_Saves()
    {
        await CreateFolder(new string('å', 250));

        var row = await DbContext.ArchiveFolders.AsNoTracking().SingleAsync();
        Assert.That(row.Name.Length, Is.EqualTo(250));
    }

    [Test]
    public async Task ArchiveFolderReadTag_Create_SavesRowAndVersion()
    {
        var folder = await CreateFolder("Ledelse");
        var readTag = new ArchiveFolderReadTag
        {
            ArchiveFolderId = folder.Id,
            SdkTagId = 4711,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };

        await readTag.Create(DbContext);

        var loaded = await DbContext.ArchiveFolders.AsNoTracking()
            .Include(f => f.ReadTags)
            .SingleAsync(f => f.Id == folder.Id);
        Assert.That(loaded.ReadTags.Select(t => t.SdkTagId), Is.EqualTo(new[] { 4711 }));
        var version = await DbContext.ArchiveFolderReadTagVersions.AsNoTracking().SingleAsync();
        Assert.That(version.ArchiveFolderReadTagId, Is.EqualTo(readTag.Id));
        Assert.That(version.ArchiveFolderId, Is.EqualTo(folder.Id));
        Assert.That(version.SdkTagId, Is.EqualTo(4711));
    }

    [Test]
    public async Task File_InFolder_SavesNewColumns_AndVersionCopiesThem()
    {
        var folder = await CreateFolder("Fakturaer");
        var file = new File
        {
            FileName = "Faktura 2026-002.pdf",
            FolderId = folder.Id,
            DocumentDate = new DateTime(2026, 9, 30),
            DocumentDateState = DocumentDateState.Suggested,
            OwnAccess = true,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };

        await file.Create(DbContext);

        var row = await DbContext.Files.AsNoTracking().Include(f => f.Folder).SingleAsync();
        Assert.That(row.FolderId, Is.EqualTo(folder.Id));
        Assert.That(row.Folder.Name, Is.EqualTo("Fakturaer"));
        Assert.That(row.DocumentDate, Is.EqualTo(new DateTime(2026, 9, 30)));
        Assert.That(row.DocumentDateState, Is.EqualTo(DocumentDateState.Suggested));
        Assert.That(row.OwnAccess, Is.True);

        var version = await DbContext.FileVersions.AsNoTracking().SingleAsync();
        Assert.That(version.FileId, Is.EqualTo(file.Id));
        Assert.That(version.FolderId, Is.EqualTo(folder.Id));
        Assert.That(version.DocumentDate, Is.EqualTo(new DateTime(2026, 9, 30)));
        Assert.That(version.DocumentDateState, Is.EqualTo(DocumentDateState.Suggested));
        Assert.That(version.OwnAccess, Is.True);
    }

    [Test]
    public async Task File_DocumentDate_DropsTimeOfDay()
    {
        var folder = await CreateFolder("Tilsyn");
        var file = new File
        {
            FileName = "Tilsynsrapport.pdf",
            FolderId = folder.Id,
            DocumentDate = new DateTime(2026, 10, 1, 23, 59, 0),
            DocumentDateState = DocumentDateState.Approved,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };

        await file.Create(DbContext);

        var row = await DbContext.Files.AsNoTracking().SingleAsync();
        Assert.That(row.DocumentDate, Is.EqualTo(new DateTime(2026, 10, 1)));

        var version = await DbContext.FileVersions.AsNoTracking().SingleAsync();
        Assert.That(version.DocumentDate, Is.EqualTo(new DateTime(2026, 10, 1)));
    }

    [Test]
    public async Task File_DefaultsAreNoDateAndInheritedAccess()
    {
        var folder = await CreateFolder("Arkiv", isDefault: true);

        await CreateFile(folder.Id);

        var row = await DbContext.Files.AsNoTracking().SingleAsync();
        Assert.That(row.DocumentDate, Is.Null);
        Assert.That(row.DocumentDateState, Is.EqualTo(DocumentDateState.None));
        Assert.That(row.OwnAccess, Is.False);
    }

    [Test]
    public async Task File_MoveToOtherFolder_WritesVersionWithNewFolder()
    {
        var from = await CreateFolder("Arkiv", isDefault: true);
        var to = await CreateFolder("Drift");
        var file = await CreateFile(from.Id);

        file.FolderId = to.Id;
        await file.Update(DbContext);

        var versions = await DbContext.FileVersions.AsNoTracking().OrderBy(v => v.Version).ToListAsync();
        Assert.That(versions.Select(v => v.FolderId), Is.EqualTo(new int?[] { from.Id, to.Id }));
    }

    [Test]
    public async Task File_WithoutFolder_LandsInTheDefaultFolder()
    {
        // The rollout safety net in AddArchiveFolders (DEFAULT 0 + TR_Files_DefaultFolder) puts a
        // file saved by old plugin code (FolderId 0) into the default folder. "Drift" is created
        // first, so the lowest id is not the default one.
        await CreateFolder("Drift");
        var arkiv = await CreateFolder("Arkiv", isDefault: true);
        var file = new File { FileName = "Uden mappe.pdf", CreatedByUserId = 0, UpdatedByUserId = 0 };

        await file.Create(DbContext);

        // EF does not read FolderId back after the insert, so the tracked entity still says 0.
        var saved = await DbContext.Files.AsNoTracking().SingleAsync(f => f.Id == file.Id);
        Assert.That(saved.FolderId, Is.EqualTo(arkiv.Id));
    }

    [Test]
    public async Task File_WithUnknownFolder_IsRejectedByTheForeignKey()
    {
        await CreateFolder("Arkiv", isDefault: true);
        var file = new File
        {
            FileName = "Forkert mappe.pdf", FolderId = 999999, CreatedByUserId = 0, UpdatedByUserId = 0
        };

        // SaveChanges wraps the provider error in DbUpdateException.
        var exception = await Assert.ThrowsAsync<DbUpdateException>(async () => await file.Create(DbContext));
        Assert.That(exception.InnerException, Is.InstanceOf<MySqlException>()
            .With.Property(nameof(MySqlException.Number)).EqualTo(ForeignKeyViolation));
    }

    [Test]
    public async Task FileReadTag_Create_SavesRowAndVersion()
    {
        var folder = await CreateFolder("Personale");
        var file = await CreateFile(folder.Id, "Ansættelseskontrakt Jane Doe.pdf");
        var readTag = new FileReadTag
        {
            FileId = file.Id,
            SdkTagId = 42,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };

        await readTag.Create(DbContext);

        var loaded = await DbContext.Files.AsNoTracking().Include(f => f.ReadTags).SingleAsync();
        Assert.That(loaded.ReadTags.Select(t => t.SdkTagId), Is.EqualTo(new[] { 42 }));
        var version = await DbContext.FileReadTagVersions.AsNoTracking().SingleAsync();
        Assert.That(version.FileReadTagId, Is.EqualTo(readTag.Id));
        Assert.That(version.FileId, Is.EqualTo(file.Id));
        Assert.That(version.SdkTagId, Is.EqualTo(42));
    }

    [Test]
    public async Task InboxSuggestion_DocumentDateKind_SavesTargetDateWithTargetIdZero()
    {
        var doc = new InboxDocument
        {
            HubDocumentId = Guid.NewGuid().ToString(),
            FromAddress = "jane.doe@example.org",
            ReceivedAt = new DateTime(2026, 10, 2, 9, 42, 0, DateTimeKind.Utc),
            FileName = "Faktura 2026-003.pdf",
            SizeBytes = 1024,
            Status = InboxDocumentStatus.Ready,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };
        await doc.Create(DbContext);
        var suggestion = new InboxSuggestion
        {
            InboxDocumentId = doc.Id,
            Kind = InboxSuggestionKind.DocumentDate,
            TargetId = 0,
            TargetDate = new DateTime(2026, 9, 28),
            Source = InboxSuggestionSource.TextMatch,
            Confidence = 0.5,
            Evidence = "Fakturadato: 28.09.2026",
            Page = 1,
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };

        await suggestion.Create(DbContext);

        var row = await DbContext.InboxSuggestions.AsNoTracking().SingleAsync();
        Assert.That(row.Kind, Is.EqualTo(InboxSuggestionKind.DocumentDate));
        Assert.That(row.TargetId, Is.EqualTo(0));
        Assert.That(row.TargetDate, Is.EqualTo(new DateTime(2026, 9, 28)));
        var version = await DbContext.InboxSuggestionVersions.AsNoTracking().SingleAsync();
        Assert.That(version.TargetDate, Is.EqualTo(new DateTime(2026, 9, 28)));
    }
}
