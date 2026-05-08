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
public class GoogleDriveIntegrationUTest : DbTestFixture
{
    private int _areaRulePlanningId;

    protected override void DoSetup()
    {
        // The shared DbTestFixture.ClearDb only truncates a fixed list that
        // doesn't include AreaRulePlannings (or the new GoogleDrive tables), so
        // wipe everything this fixture exercises before each run.
        DbContext.Database.ExecuteSqlRaw(
            "SET FOREIGN_KEY_CHECKS = 0; " +
            "TRUNCATE `DriveWatchChannelVersions`; " +
            "TRUNCATE `DriveWatchChannels`; " +
            "TRUNCATE `AreaRulePlanningFileVersions`; " +
            "TRUNCATE `AreaRulePlanningFiles`; " +
            "TRUNCATE `GoogleOAuthTokenVersions`; " +
            "TRUNCATE `GoogleOAuthTokens`; " +
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

    private static GoogleOAuthToken NewToken(
        int userId = 7,
        string email = "user@example.com",
        string encryptedRefreshToken = "ENCRYPTED-REFRESH",
        DateTime? connectedAt = null,
        DateTime? lastUsedAt = null,
        DateTime? revokedAt = null) => new()
    {
        UserId = userId,
        GoogleAccountEmail = email,
        EncryptedRefreshToken = encryptedRefreshToken,
        ConnectedAt = connectedAt ?? new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
        LastUsedAt = lastUsedAt,
        RevokedAt = revokedAt,
        CreatedByUserId = 1,
        UpdatedByUserId = 1,
    };

    [Test]
    public async Task GoogleOAuthToken_Create_PersistsAllFieldsAndCreatesVersion()
    {
        // Arrange — populate every scalar (including the nullables) so the
        // round-trip exercises the full column set.
        var connectedAt = new DateTime(2026, 5, 1, 8, 30, 0, DateTimeKind.Utc);
        var lastUsedAt = new DateTime(2026, 5, 5, 14, 15, 0, DateTimeKind.Utc);
        var revokedAt = new DateTime(2026, 5, 6, 9, 0, 0, DateTimeKind.Utc);
        var token = NewToken(
            userId: 42,
            email: "alice@example.com",
            encryptedRefreshToken: "AES-GCM-CIPHERTEXT",
            connectedAt: connectedAt,
            lastUsedAt: lastUsedAt,
            revokedAt: revokedAt);

        // Act
        await token.Create(DbContext);

        var rows = DbContext.GoogleOAuthTokens.AsNoTracking().ToList();
        var versions = DbContext.GoogleOAuthTokenVersions.AsNoTracking().ToList();

        // Assert — main row persisted with all fields round-tripped.
        Assert.That(rows.Count, Is.EqualTo(1));
        Assert.That(rows[0].UserId, Is.EqualTo(42));
        Assert.That(rows[0].GoogleAccountEmail, Is.EqualTo("alice@example.com"));
        Assert.That(rows[0].EncryptedRefreshToken, Is.EqualTo("AES-GCM-CIPHERTEXT"));
        Assert.That(rows[0].ConnectedAt, Is.EqualTo(connectedAt));
        Assert.That(rows[0].LastUsedAt, Is.EqualTo(lastUsedAt));
        Assert.That(rows[0].RevokedAt, Is.EqualTo(revokedAt));
        Assert.That(rows[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(rows[0].Version, Is.EqualTo(1));

        // Version row written by PnBase.Create — same values mirrored,
        // GoogleOAuthTokenId points back to the main row.
        Assert.That(versions.Count, Is.EqualTo(1));
        Assert.That(versions[0].GoogleOAuthTokenId, Is.EqualTo(rows[0].Id));
        Assert.That(versions[0].UserId, Is.EqualTo(42));
        Assert.That(versions[0].GoogleAccountEmail, Is.EqualTo("alice@example.com"));
        Assert.That(versions[0].EncryptedRefreshToken, Is.EqualTo("AES-GCM-CIPHERTEXT"));
        Assert.That(versions[0].ConnectedAt, Is.EqualTo(connectedAt));
        Assert.That(versions[0].LastUsedAt, Is.EqualTo(lastUsedAt));
        Assert.That(versions[0].RevokedAt, Is.EqualTo(revokedAt));
    }

    [Test]
    public async Task GoogleOAuthToken_Delete_SoftDeletesAndIncrementsVersion()
    {
        // Arrange
        var token = NewToken();
        await token.Create(DbContext);

        // Act
        await token.Delete(DbContext);

        var rows = DbContext.GoogleOAuthTokens.AsNoTracking().ToList();
        var versions = DbContext.GoogleOAuthTokenVersions
            .AsNoTracking()
            .OrderBy(v => v.Version)
            .ToList();

        // Assert — main row stays, just flipped to Removed; version bumped to 2.
        Assert.That(rows.Count, Is.EqualTo(1));
        Assert.That(rows[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(rows[0].Version, Is.EqualTo(2));

        // Two version rows: initial Created and the Removed mirror.
        Assert.That(versions.Count, Is.EqualTo(2));
        Assert.That(versions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(versions[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
    }

    [Test]
    public async Task DriveWatchChannel_Create_PersistsAllFieldsAndLinksToToken()
    {
        // Arrange — token first so the channel's FK has somewhere to point.
        var token = NewToken();
        await token.Create(DbContext);

        var expiresAt = new DateTime(2026, 5, 14, 12, 0, 0, DateTimeKind.Utc);
        var channel = new DriveWatchChannel
        {
            GoogleOAuthTokenId = token.Id,
            ChannelId = "channel-uuid-123",
            ResourceId = "resource-id-from-google",
            SignedToken = "JWT.PAYLOAD.SIGNATURE",
            ExpiresAt = expiresAt,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await channel.Create(DbContext);

        var rows = DbContext.DriveWatchChannels.AsNoTracking().ToList();
        var versions = DbContext.DriveWatchChannelVersions.AsNoTracking().ToList();

        // Assert — every scalar round-trips.
        Assert.That(rows.Count, Is.EqualTo(1));
        Assert.That(rows[0].GoogleOAuthTokenId, Is.EqualTo(token.Id));
        Assert.That(rows[0].ChannelId, Is.EqualTo("channel-uuid-123"));
        Assert.That(rows[0].ResourceId, Is.EqualTo("resource-id-from-google"));
        Assert.That(rows[0].SignedToken, Is.EqualTo("JWT.PAYLOAD.SIGNATURE"));
        Assert.That(rows[0].ExpiresAt, Is.EqualTo(expiresAt));

        // Version row mirrors scalars + parent id.
        Assert.That(versions.Count, Is.EqualTo(1));
        Assert.That(versions[0].DriveWatchChannelId, Is.EqualTo(rows[0].Id));
        Assert.That(versions[0].GoogleOAuthTokenId, Is.EqualTo(token.Id));
        Assert.That(versions[0].ChannelId, Is.EqualTo("channel-uuid-123"));
        Assert.That(versions[0].SignedToken, Is.EqualTo("JWT.PAYLOAD.SIGNATURE"));

        // Confirm the nav loads from the token side.
        var loaded = DbContext.GoogleOAuthTokens
            .Include(t => t.DriveWatchChannels)
            .Single(t => t.Id == token.Id);
        Assert.That(loaded.DriveWatchChannels.Count, Is.EqualTo(1));
        Assert.That(loaded.DriveWatchChannels.First().ChannelId, Is.EqualTo("channel-uuid-123"));
    }

    [Test]
    public void DriveWatchChannel_Create_WithoutValidGoogleOAuthTokenId_FailsFkConstraint()
    {
        // Arrange — point at a token id that does not exist; the FK on
        // DriveWatchChannels.GoogleOAuthTokenId must reject it.
        var orphan = new DriveWatchChannel
        {
            GoogleOAuthTokenId = 999_999,
            ChannelId = "channel-orphan",
            ResourceId = "resource-orphan",
            SignedToken = "JWT.ORPHAN",
            ExpiresAt = new DateTime(2026, 5, 14, 12, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act + Assert — Pomelo wraps the MySQL FK violation as a DbUpdateException.
        Assert.ThrowsAsync<DbUpdateException>(async () => await orphan.Create(DbContext));
    }

    [Test]
    public async Task AreaRulePlanningFile_WithDriveFields_PersistsRoundTrip()
    {
        // Arrange — token must exist before the file can reference it.
        var token = NewToken(email: "drive-user@example.com");
        await token.Create(DbContext);

        var driveModifiedTime = new DateTime(2026, 5, 7, 10, 30, 0, DateTimeKind.Utc);
        var file = new AreaRulePlanningFile
        {
            AreaRulePlanningId = _areaRulePlanningId,
            UploadedDataId = 42,
            OriginalFileName = "drive-doc.pdf",
            MimeType = "application/pdf",
            SizeBytes = 8192,
            DriveFileId = "1A2B3C4D-drive-file-id",
            DriveModifiedTime = driveModifiedTime,
            GoogleOAuthTokenId = token.Id,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await file.Create(DbContext);

        // Assert — drive fields round-trip on both the main row and the version.
        var loaded = DbContext.AreaRulePlanningFiles
            .Include(f => f.GoogleOAuthToken)
            .AsNoTracking()
            .Single();
        Assert.That(loaded.DriveFileId, Is.EqualTo("1A2B3C4D-drive-file-id"));
        Assert.That(loaded.DriveModifiedTime, Is.EqualTo(driveModifiedTime));
        Assert.That(loaded.GoogleOAuthTokenId, Is.EqualTo(token.Id));
        Assert.That(loaded.GoogleOAuthToken, Is.Not.Null);
        Assert.That(loaded.GoogleOAuthToken!.GoogleAccountEmail, Is.EqualTo("drive-user@example.com"));

        var version = DbContext.AreaRulePlanningFileVersions.AsNoTracking().Single();
        Assert.That(version.DriveFileId, Is.EqualTo("1A2B3C4D-drive-file-id"));
        Assert.That(version.DriveModifiedTime, Is.EqualTo(driveModifiedTime));
        Assert.That(version.GoogleOAuthTokenId, Is.EqualTo(token.Id));
    }

    [Test]
    public void AreaRulePlanningFile_WithBadGoogleOAuthTokenId_FailsFkConstraint()
    {
        // Arrange — point AreaRulePlanningFile.GoogleOAuthTokenId at a token
        // id that does not exist; the FK must reject it. This locks the
        // contract that PR-3+ relies on (orphaned drive-attached files
        // cannot be created without a backing token row).
        var orphan = new AreaRulePlanningFile
        {
            AreaRulePlanningId = _areaRulePlanningId,
            UploadedDataId = 99,
            OriginalFileName = "orphan-drive.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            DriveFileId = "orphan-drive-id",
            DriveModifiedTime = new DateTime(2026, 5, 7, 10, 30, 0, DateTimeKind.Utc),
            GoogleOAuthTokenId = 999_999,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        Assert.ThrowsAsync<DbUpdateException>(async () => await orphan.Create(DbContext));
    }
}
