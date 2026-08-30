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
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class DeviceTokenUTest : DbTestFixture
{
    private const string AdhocAppId = "adhoc";

    private static string GetRandomStr() => Guid.NewGuid().ToString();

    private static DeviceToken NewDeviceToken(
        string installationId,
        string appId = AdhocAppId,
        string fcmToken = null,
        int sdkSiteId = 42,
        string platform = "android")
    {
        return new DeviceToken
        {
            AppId = appId,
            InstallationId = installationId,
            FcmToken = fcmToken ?? GetRandomStr(),
            SdkSiteId = sdkSiteId,
            Platform = platform,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
    }

    [Test]
    public async Task DeviceToken_Create_DoesSave()
    {
        // Arrange
        var deviceToken = NewDeviceToken("install-create");

        // Act
        await deviceToken.Create(DbContext);

        var deviceTokenList = DbContext.DeviceTokens.AsNoTracking().ToList();
        var deviceTokenVersionList = DbContext.DeviceTokenVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(deviceTokenList.Count, Is.EqualTo(1));
        Assert.That(deviceTokenVersionList.Count, Is.EqualTo(1));
        Assert.That(deviceTokenList[0].AppId, Is.EqualTo(AdhocAppId));
        Assert.That(deviceTokenList[0].InstallationId, Is.EqualTo("install-create"));
        Assert.That(deviceTokenList[0].SdkSiteId, Is.EqualTo(42));
        Assert.That(deviceTokenList[0].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenList[0].Platform, Is.EqualTo("android"));
        Assert.That(deviceTokenList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(deviceTokenList[0].Version, Is.EqualTo(1));

        Assert.That(deviceTokenVersionList[0].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[0].AppId, Is.EqualTo(AdhocAppId));
        Assert.That(deviceTokenVersionList[0].InstallationId, Is.EqualTo("install-create"));
        Assert.That(deviceTokenVersionList[0].SdkSiteId, Is.EqualTo(42));
        Assert.That(deviceTokenVersionList[0].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenVersionList[0].Platform, Is.EqualTo("android"));
        Assert.That(deviceTokenVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(deviceTokenVersionList[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task DeviceToken_Create_NullAppId_Throws()
    {
        // AppId is [Required] so that EF maps it NOT NULL: a MariaDB unique
        // index permits unlimited rows whose indexed columns are NULL, so a
        // nullable AppId would silently defeat the (AppId, InstallationId)
        // identity. This pins that the database rejects the NULL, so dropping
        // [Required] as tidy-up cannot pass unnoticed.
        var deviceToken = NewDeviceToken("install-null-appid", appId: null);

        Assert.ThrowsAsync<DbUpdateException>(async () => await deviceToken.Create(DbContext));
    }

    [Test]
    public async Task DeviceToken_Update_DoesUpdate()
    {
        // Arrange
        var deviceToken = NewDeviceToken("install-update");

        await deviceToken.Create(DbContext);
        var deviceTokenOld = DbContext.DeviceTokens.AsNoTracking().First();

        // Act - flip every mutable column. AppId/InstallationId are the row's
        // identity and deliberately stay put: a rotated token and a new owner
        // must land on the same row.
        deviceToken.FcmToken = GetRandomStr();
        deviceToken.SdkSiteId = 43;
        deviceToken.Platform = "ios";
        deviceToken.UpdatedByUserId = 2;

        await deviceToken.Update(DbContext);

        var deviceTokenList = DbContext.DeviceTokens.AsNoTracking().ToList();
        var deviceTokenVersionList = DbContext.DeviceTokenVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(deviceTokenList.Count, Is.EqualTo(1));
        Assert.That(deviceTokenVersionList.Count, Is.EqualTo(2));
        Assert.That(deviceTokenList[0].Id, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenList[0].AppId, Is.EqualTo(AdhocAppId));
        Assert.That(deviceTokenList[0].InstallationId, Is.EqualTo("install-update"));
        Assert.That(deviceTokenList[0].SdkSiteId, Is.EqualTo(43));
        Assert.That(deviceTokenList[0].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenList[0].Platform, Is.EqualTo("ios"));
        Assert.That(deviceTokenList[0].UpdatedByUserId, Is.EqualTo(2));
        Assert.That(deviceTokenList[0].Version, Is.EqualTo(2));

        // Pre-mutation snapshot preserved at the old values
        Assert.That(deviceTokenVersionList[0].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[0].AppId, Is.EqualTo(AdhocAppId));
        Assert.That(deviceTokenVersionList[0].InstallationId, Is.EqualTo("install-update"));
        Assert.That(deviceTokenVersionList[0].SdkSiteId, Is.EqualTo(deviceTokenOld.SdkSiteId));
        Assert.That(deviceTokenVersionList[0].FcmToken, Is.EqualTo(deviceTokenOld.FcmToken));
        Assert.That(deviceTokenVersionList[0].Platform, Is.EqualTo(deviceTokenOld.Platform));
        Assert.That(deviceTokenVersionList[0].Version, Is.EqualTo(1));

        // Post-mutation snapshot matches every new value
        Assert.That(deviceTokenVersionList[1].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[1].AppId, Is.EqualTo(AdhocAppId));
        Assert.That(deviceTokenVersionList[1].InstallationId, Is.EqualTo("install-update"));
        Assert.That(deviceTokenVersionList[1].SdkSiteId, Is.EqualTo(43));
        Assert.That(deviceTokenVersionList[1].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenVersionList[1].Platform, Is.EqualTo("ios"));
        Assert.That(deviceTokenVersionList[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task DeviceToken_Create_SameTokenSameSite_DifferentInstalls_DoesSave()
    {
        // FcmToken is not an identity: the same token string may sit on more
        // than one row, and IX_DeviceTokens_FcmToken is a lookup index, NOT
        // unique. Both rows share an SdkSiteId as well as the token - the
        // shared-device case the old (WorkerId, FcmToken) unique key rejected
        // and this model exists to permit.
        var fcmToken = GetRandomStr();

        var deviceTokenOne = NewDeviceToken(
            "install-shared-token-1",
            fcmToken: fcmToken,
            sdkSiteId: 1);

        var deviceTokenTwo = NewDeviceToken(
            "install-shared-token-2",
            fcmToken: fcmToken,
            sdkSiteId: 1);

        // Act
        await deviceTokenOne.Create(DbContext);
        await deviceTokenTwo.Create(DbContext);

        // Assert
        Assert.That(DbContext.DeviceTokens.AsNoTracking().Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task DeviceToken_Create_DuplicateAppAndInstallation_Throws()
    {
        // The unique key is enforced by the database, not merely by convention
        // in the consumer. The two rows differ in FcmToken AND SdkSiteId, so
        // only (AppId, InstallationId) can be what rejects them.
        var deviceTokenOne = NewDeviceToken("install-dup", sdkSiteId: 1);
        var deviceTokenTwo = NewDeviceToken("install-dup", sdkSiteId: 2);

        await deviceTokenOne.Create(DbContext);

        // Act & Assert
        Assert.ThrowsAsync<DbUpdateException>(async () => await deviceTokenTwo.Create(DbContext));
    }

    [Test]
    public async Task DeviceToken_Delete_DoesDelete()
    {
        // Arrange
        var deviceToken = NewDeviceToken("install-delete");

        await deviceToken.Create(DbContext);

        // Act
        await deviceToken.Delete(DbContext);

        var deviceTokenList = DbContext.DeviceTokens.AsNoTracking().ToList();
        var deviceTokenVersionList = DbContext.DeviceTokenVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(deviceTokenList.Count, Is.EqualTo(1));
        Assert.That(deviceTokenVersionList.Count, Is.EqualTo(2));
        Assert.That(deviceTokenList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(deviceTokenList[0].Id, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenList[0].Version, Is.EqualTo(2));

        Assert.That(deviceTokenVersionList[0].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(deviceTokenVersionList[0].Version, Is.EqualTo(1));

        Assert.That(deviceTokenVersionList[1].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(deviceTokenVersionList[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task DeviceToken_SameInstall_NewToken_UpdatesInPlace()
    {
        // A rotated FCM token must land on the existing row, not create one.
        var deviceToken = NewDeviceToken(
            "install-1",
            fcmToken: "tok-old",
            sdkSiteId: 100);

        await deviceToken.Create(DbContext);

        deviceToken.FcmToken = "tok-new";
        await deviceToken.Update(DbContext);

        var deviceTokenList = DbContext.DeviceTokens.AsNoTracking().ToList();

        Assert.That(deviceTokenList, Has.Count.EqualTo(1));
        Assert.That(deviceTokenList[0].FcmToken, Is.EqualTo("tok-new"));
        Assert.That(deviceTokenList[0].InstallationId, Is.EqualTo("install-1"));
    }

    [Test]
    public async Task DeviceToken_SameInstall_DifferentApp_IsASeparateRow()
    {
        // AppId is half the key: the same InstallationId under two apps is two
        // rows, so one app's registration can never overwrite the other's.
        var deviceTokenAdhoc = NewDeviceToken(
            "shared-install",
            fcmToken: "tok-a",
            sdkSiteId: 101);

        var deviceTokenEform = NewDeviceToken(
            "shared-install",
            appId: "eform",
            fcmToken: "tok-b",
            sdkSiteId: 101);

        await deviceTokenAdhoc.Create(DbContext);
        await deviceTokenEform.Create(DbContext);

        Assert.That(DbContext.DeviceTokens.AsNoTracking().Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task DeviceToken_VersionRow_CarriesNewColumns()
    {
        // PnBase.MapVersion copies by property name via reflection and
        // swallows a mismatch per property, so a renamed column drops out of
        // the audit trail silently. This is the test that notices.
        var deviceToken = NewDeviceToken(
            "install-v",
            fcmToken: "tok-v",
            sdkSiteId: 102,
            platform: "ios");

        await deviceToken.Create(DbContext);

        var deviceTokenVersion = DbContext.DeviceTokenVersions.AsNoTracking().Single();

        Assert.That(deviceTokenVersion.AppId, Is.EqualTo(AdhocAppId));
        Assert.That(deviceTokenVersion.InstallationId, Is.EqualTo("install-v"));
        Assert.That(deviceTokenVersion.SdkSiteId, Is.EqualTo(102));
    }
}
