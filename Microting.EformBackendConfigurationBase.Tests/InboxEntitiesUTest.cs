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
using eForm.Infrastructure.Constants;
using Infrastructure.Data.Entities;
using Infrastructure.Enum;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class InboxEntitiesUTest : DbTestFixture
{
    private static InboxDocument NewDocument(string hubId = null) => new()
    {
        HubDocumentId = hubId ?? Guid.NewGuid().ToString(),
        FromAddress = "jane.doe@example.org",
        Subject = "Fwd: Servicerapport ventilationsanlæg",
        ReceivedAt = new DateTime(2026, 10, 2, 9, 42, 0, DateTimeKind.Utc),
        FileName = "Servicerapport VA-02.pdf",
        SizeBytes = 421_888,
        Status = InboxDocumentStatus.Preparing,
        CreatedByUserId = 0,
        UpdatedByUserId = 0
    };

    [Test]
    public async Task InboxDocument_Create_SavesRowAndVersion()
    {
        var doc = NewDocument();

        await doc.Create(DbContext);

        var rows = await DbContext.InboxDocuments.AsNoTracking().ToListAsync();
        var versions = await DbContext.InboxDocumentVersions.AsNoTracking().ToListAsync();
        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].Status, Is.EqualTo(InboxDocumentStatus.Preparing));
        Assert.That(rows[0].Subject, Is.EqualTo("Fwd: Servicerapport ventilationsanlæg"));
        Assert.That(versions, Has.Count.EqualTo(1));
        Assert.That(versions[0].InboxDocumentId, Is.EqualTo(doc.Id));
        Assert.That(versions[0].HubDocumentId, Is.EqualTo(doc.HubDocumentId));
    }

    [Test]
    public async Task InboxDocument_UpdateStatus_WritesSecondVersion()
    {
        var doc = NewDocument();
        await doc.Create(DbContext);

        doc.Status = InboxDocumentStatus.Ready;
        doc.Md5 = "0123456789abcdef0123456789abcdef";
        await doc.Update(DbContext);

        var versions = await DbContext.InboxDocumentVersions.AsNoTracking()
            .OrderBy(v => v.Version).ToListAsync();
        Assert.That(versions, Has.Count.EqualTo(2));
        Assert.That(versions[1].Status, Is.EqualTo(InboxDocumentStatus.Ready));
        Assert.That(versions[1].Md5, Is.EqualTo("0123456789abcdef0123456789abcdef"));
    }

    [Test]
    public async Task InboxDocument_Delete_IsSoft()
    {
        var doc = NewDocument();
        await doc.Create(DbContext);

        await doc.Delete(DbContext);

        var row = await DbContext.InboxDocuments.AsNoTracking().SingleAsync();
        Assert.That(row.WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(await DbContext.InboxDocumentVersions.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task InboxDocument_LongSubject_Saves()
    {
        var doc = NewDocument();
        doc.Subject = new string('æ', 500);

        await doc.Create(DbContext);

        var row = await DbContext.InboxDocuments.AsNoTracking().SingleAsync();
        Assert.That(row.Subject.Length, Is.EqualTo(500));
    }

    [Test]
    public async Task InboxDocument_DuplicateHubDocumentId_Throws()
    {
        var hubId = Guid.NewGuid().ToString();
        await NewDocument(hubId).Create(DbContext);

        Assert.ThrowsAsync<DbUpdateException>(async () => await NewDocument(hubId).Create(DbContext));
    }

    [Test]
    public async Task InboxSuggestion_Create_KeepsDanishEvidence()
    {
        var doc = NewDocument();
        await doc.Create(DbContext);
        var suggestion = new InboxSuggestion
        {
            InboxDocumentId = doc.Id,
            Kind = InboxSuggestionKind.Property,
            TargetId = 12,
            Source = InboxSuggestionSource.TextMatch,
            Confidence = 0.5,
            Evidence = "Leveringsadresse: Søndervej 4, 8600 Silkeborg",
            Page = 1,
            Reason = "Adressen står i dokumentet.",
            CreatedByUserId = 0,
            UpdatedByUserId = 0
        };

        await suggestion.Create(DbContext);

        var row = await DbContext.InboxSuggestions.AsNoTracking().SingleAsync();
        Assert.That(row.Evidence, Is.EqualTo("Leveringsadresse: Søndervej 4, 8600 Silkeborg"));
        Assert.That(row.Accepted, Is.Null);
        var version = await DbContext.InboxSuggestionVersions.AsNoTracking().SingleAsync();
        Assert.That(version.InboxSuggestionId, Is.EqualTo(suggestion.Id));
        Assert.That(version.InboxDocumentId, Is.EqualTo(doc.Id));
    }

    [Test]
    public async Task InboxAddress_TwoRows_DifferentHash_BothSave()
    {
        var oldAddress = new InboxAddress
        {
            Address = "4711-k7f2q9abcd@indbakke.example.test",
            TokenHash = new string('a', 64),
            Active = false,
            GraceUntil = new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 1, UpdatedByUserId = 1
        };
        var newAddress = new InboxAddress
        {
            Address = "4711-p3xm8defgh@indbakke.example.test",
            TokenHash = new string('b', 64),
            Active = true,
            CreatedByUserId = 1, UpdatedByUserId = 1
        };

        await oldAddress.Create(DbContext);
        await newAddress.Create(DbContext);

        Assert.That(await DbContext.InboxAddresses.CountAsync(), Is.EqualTo(2));
        Assert.That(await DbContext.InboxAddressVersions.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task InboxSenderRule_Create_SavesKind()
    {
        var rule = new InboxSenderRule
        {
            Pattern = "@example.org",
            Kind = InboxSenderRuleKind.Allow,
            CreatedByUserId = 1, UpdatedByUserId = 1
        };

        await rule.Create(DbContext);

        var row = await DbContext.InboxSenderRules.AsNoTracking().SingleAsync();
        Assert.That(row.Kind, Is.EqualTo(InboxSenderRuleKind.Allow));
        Assert.That(await DbContext.InboxSenderRuleVersions.CountAsync(), Is.EqualTo(1));
    }
}
