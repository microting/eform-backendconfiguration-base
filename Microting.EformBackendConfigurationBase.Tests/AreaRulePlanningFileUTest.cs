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
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure.Constants;
using NUnit.Framework;

[TestFixture]
public class AreaRulePlanningFileUTest : DbTestFixture
{
    private int _areaRulePlanningId;

    protected override void DoSetup()
    {
        // The shared DbTestFixture.ClearDb only truncates a fixed list that
        // doesn't include AreaRulePlannings (or the new file tables), so wipe
        // everything this test exercises before each run.
        DbContext.Database.ExecuteSqlRaw(
            "SET FOREIGN_KEY_CHECKS = 0; " +
            "TRUNCATE `AreaRulePlanningFileVersions`; " +
            "TRUNCATE `AreaRulePlanningFiles`; " +
            "TRUNCATE `AreaRulesPlanningVersions`; " +
            "TRUNCATE `AreaRulePlannings`; " +
            "TRUNCATE `AreaRuleVersions`; " +
            "TRUNCATE `AreaRules`; " +
            "TRUNCATE `AreaInitialFieldVersions`; " +
            "TRUNCATE `AreaInitialFields`; " +
            "TRUNCATE `AreaTranslationVersions`; " +
            "TRUNCATE `AreaTranslations`; " +
            "TRUNCATE `AreaVersions`; " +
            "TRUNCATE `Areas`; " +
            "TRUNCATE `PropertieVersions`; " +
            "TRUNCATE `Properties`;");

        // Build a minimal Property → Area → AreaRule → AreaRulePlanning chain so
        // AreaRulePlanningFile can satisfy its non-nullable FK to AreaRulePlanning.
        var property = new Property
        {
            Address = Guid.NewGuid().ToString(),
            CHR = Guid.NewGuid().ToString(),
            Name = Guid.NewGuid().ToString(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        property.Create(DbContext).GetAwaiter().GetResult();

        var area = new Area
        {
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        area.Create(DbContext).GetAwaiter().GetResult();

        var areaRule = new AreaRule
        {
            AreaId = area.Id,
            PropertyId = property.Id,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        areaRule.Create(DbContext).GetAwaiter().GetResult();

        var areaRulePlanning = new AreaRulePlanning
        {
            AreaRuleId = areaRule.Id,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        areaRulePlanning.Create(DbContext).GetAwaiter().GetResult();

        _areaRulePlanningId = areaRulePlanning.Id;
    }

    private AreaRulePlanningFile NewFile(
        string originalFileName = "report.pdf",
        string mimeType = "application/pdf",
        long sizeBytes = 12_345,
        int uploadedDataId = 99) => new()
    {
        AreaRulePlanningId = _areaRulePlanningId,
        UploadedDataId = uploadedDataId,
        OriginalFileName = originalFileName,
        MimeType = mimeType,
        SizeBytes = sizeBytes,
        CreatedByUserId = 1,
        UpdatedByUserId = 1,
    };

    [Test]
    public async Task AreaRulePlanningFile_Create_PersistsAndReadsBack()
    {
        // Arrange
        var file = NewFile(
            originalFileName: "first.pdf",
            mimeType: "application/pdf",
            sizeBytes: 4096,
            uploadedDataId: 42);

        // Act
        await file.Create(DbContext);

        var fileList = DbContext.AreaRulePlanningFiles.AsNoTracking().ToList();
        var versionList = DbContext.AreaRulePlanningFileVersions.AsNoTracking().ToList();

        // Assert — main row persisted with all fields round-tripped.
        Assert.That(fileList.Count, Is.EqualTo(1));
        Assert.That(fileList[0].AreaRulePlanningId, Is.EqualTo(_areaRulePlanningId));
        Assert.That(fileList[0].UploadedDataId, Is.EqualTo(42));
        Assert.That(fileList[0].OriginalFileName, Is.EqualTo("first.pdf"));
        Assert.That(fileList[0].MimeType, Is.EqualTo("application/pdf"));
        Assert.That(fileList[0].SizeBytes, Is.EqualTo(4096));
        Assert.That(fileList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(fileList[0].Version, Is.EqualTo(1));

        // Version row written by PnBase.Create.
        Assert.That(versionList.Count, Is.EqualTo(1));
        Assert.That(versionList[0].AreaRulePlanningFileId, Is.EqualTo(fileList[0].Id));
        Assert.That(versionList[0].OriginalFileName, Is.EqualTo("first.pdf"));

        // Confirm via the AreaRulePlanning navigation property.
        var arp = DbContext.AreaRulePlannings
            .Include(p => p.AreaRulePlanningFiles)
            .Single(p => p.Id == _areaRulePlanningId);
        Assert.That(arp.AreaRulePlanningFiles.Count, Is.EqualTo(1));
        Assert.That(arp.AreaRulePlanningFiles.First().OriginalFileName, Is.EqualTo("first.pdf"));
    }

    [Test]
    public async Task AreaRulePlanningFile_Update_PersistsNewVersionRow()
    {
        // Arrange
        var file = NewFile(originalFileName: "old.pdf");
        await file.Create(DbContext);

        // Act — change OriginalFileName, persist update.
        file.OriginalFileName = "new.pdf";
        await file.Update(DbContext);

        var fileList = DbContext.AreaRulePlanningFiles.AsNoTracking().ToList();
        var versionList = DbContext.AreaRulePlanningFileVersions
            .AsNoTracking()
            .OrderBy(v => v.Version)
            .ToList();

        // Assert
        Assert.That(fileList.Count, Is.EqualTo(1));
        Assert.That(fileList[0].OriginalFileName, Is.EqualTo("new.pdf"));
        Assert.That(fileList[0].Version, Is.EqualTo(2));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(versionList[0].OriginalFileName, Is.EqualTo("old.pdf"));
        Assert.That(versionList[1].OriginalFileName, Is.EqualTo("new.pdf"));
    }

    [Test]
    public async Task AreaRulePlanningFile_Delete_SoftDeletesByWorkflowState()
    {
        // Arrange
        var file = NewFile();
        await file.Create(DbContext);

        // Act
        await file.Delete(DbContext);

        var fileList = DbContext.AreaRulePlanningFiles.AsNoTracking().ToList();
        var versionList = DbContext.AreaRulePlanningFileVersions
            .AsNoTracking()
            .OrderBy(v => v.Version)
            .ToList();

        // Assert — main row stays, just flipped to Removed.
        Assert.That(fileList.Count, Is.EqualTo(1));
        Assert.That(fileList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(fileList[0].Version, Is.EqualTo(2));

        // Two version rows: initial Created and the Removed mirror.
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(versionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(versionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
    }

    [Test]
    public async Task AreaRulePlanningFile_MaxLengthBoundary_FilenameAt255()
    {
        // Arrange — column is varchar(255) / varchar(50); persist at the cap to prove
        // the column widths fit the longest valid input.
        var maxFilename = new string('a', 255);
        var maxMime = new string('m', 50);
        var file = NewFile(
            originalFileName: maxFilename,
            mimeType: maxMime,
            sizeBytes: 1024);

        // Act
        await file.Create(DbContext);

        var fileList = DbContext.AreaRulePlanningFiles.AsNoTracking().ToList();

        // Assert — exact round-trip, no truncation.
        Assert.That(fileList.Count, Is.EqualTo(1));
        Assert.That(fileList[0].OriginalFileName, Is.EqualTo(maxFilename));
        Assert.That(fileList[0].OriginalFileName.Length, Is.EqualTo(255));
        Assert.That(fileList[0].MimeType, Is.EqualTo(maxMime));
        Assert.That(fileList[0].MimeType.Length, Is.EqualTo(50));
    }

    [Test]
    public async Task AreaRulePlanningFile_MultipleFilesPerPlanning()
    {
        // Arrange — persist 5 files, soft-delete one to prove the consumer-side
        // WorkflowState filter behaves as expected.
        for (var i = 0; i < 5; i++)
        {
            var file = NewFile(
                originalFileName: $"file{i}.pdf",
                uploadedDataId: 100 + i);
            await file.Create(DbContext);
        }

        var toRemove = DbContext.AreaRulePlanningFiles.First(f => f.OriginalFileName == "file2.pdf");
        await toRemove.Delete(DbContext);

        // Act — query through the AreaRulePlanning nav, applying the consumer-side
        // soft-delete filter.
        var arp = DbContext.AreaRulePlannings
            .Include(p => p.AreaRulePlanningFiles)
            .Single(p => p.Id == _areaRulePlanningId);

        var activeFiles = arp.AreaRulePlanningFiles
            .Where(f => f.WorkflowState != Constants.WorkflowStates.Removed)
            .OrderBy(f => f.OriginalFileName)
            .ToList();

        // Assert
        Assert.That(arp.AreaRulePlanningFiles.Count, Is.EqualTo(5)); // soft-delete keeps row
        Assert.That(activeFiles.Count, Is.EqualTo(4));
        Assert.That(activeFiles.Select(f => f.OriginalFileName), Is.EquivalentTo(new[]
        {
            "file0.pdf", "file1.pdf", "file3.pdf", "file4.pdf",
        }));
    }
}
