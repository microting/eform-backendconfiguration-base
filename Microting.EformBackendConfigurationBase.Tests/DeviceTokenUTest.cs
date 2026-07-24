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
    private static string GetRandomStr() => Guid.NewGuid().ToString();

    [Test]
    public async Task DeviceToken_Create_DoesSave()
    {
        // Arrange
        var deviceToken = new DeviceToken
        {
            WorkerId = 42,
            FcmToken = GetRandomStr(),
            Platform = "android",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await deviceToken.Create(DbContext);

        var deviceTokenList = DbContext.DeviceTokens.AsNoTracking().ToList();
        var deviceTokenVersionList = DbContext.DeviceTokenVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(deviceTokenList.Count, Is.EqualTo(1));
        Assert.That(deviceTokenVersionList.Count, Is.EqualTo(1));
        Assert.That(deviceTokenList[0].WorkerId, Is.EqualTo(42));
        Assert.That(deviceTokenList[0].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenList[0].Platform, Is.EqualTo("android"));
        Assert.That(deviceTokenList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(deviceTokenList[0].Version, Is.EqualTo(1));

        Assert.That(deviceTokenVersionList[0].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[0].WorkerId, Is.EqualTo(42));
        Assert.That(deviceTokenVersionList[0].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenVersionList[0].Platform, Is.EqualTo("android"));
        Assert.That(deviceTokenVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(deviceTokenVersionList[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task DeviceToken_Update_DoesUpdate()
    {
        // Arrange
        var deviceToken = new DeviceToken
        {
            WorkerId = 42,
            FcmToken = GetRandomStr(),
            Platform = "android",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await deviceToken.Create(DbContext);
        var deviceTokenOld = DbContext.DeviceTokens.AsNoTracking().First();

        // Act - flip every column to a new value
        deviceToken.WorkerId = 43;
        deviceToken.FcmToken = GetRandomStr();
        deviceToken.Platform = "ios";
        deviceToken.UpdatedByUserId = 2;

        await deviceToken.Update(DbContext);

        var deviceTokenList = DbContext.DeviceTokens.AsNoTracking().ToList();
        var deviceTokenVersionList = DbContext.DeviceTokenVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(deviceTokenList.Count, Is.EqualTo(1));
        Assert.That(deviceTokenVersionList.Count, Is.EqualTo(2));
        Assert.That(deviceTokenList[0].Id, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenList[0].WorkerId, Is.EqualTo(43));
        Assert.That(deviceTokenList[0].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenList[0].Platform, Is.EqualTo("ios"));
        Assert.That(deviceTokenList[0].UpdatedByUserId, Is.EqualTo(2));
        Assert.That(deviceTokenList[0].Version, Is.EqualTo(2));

        // Pre-mutation snapshot preserved at the old values
        Assert.That(deviceTokenVersionList[0].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[0].WorkerId, Is.EqualTo(deviceTokenOld.WorkerId));
        Assert.That(deviceTokenVersionList[0].FcmToken, Is.EqualTo(deviceTokenOld.FcmToken));
        Assert.That(deviceTokenVersionList[0].Platform, Is.EqualTo(deviceTokenOld.Platform));
        Assert.That(deviceTokenVersionList[0].Version, Is.EqualTo(1));

        // Post-mutation snapshot matches every new value
        Assert.That(deviceTokenVersionList[1].DeviceTokenId, Is.EqualTo(deviceToken.Id));
        Assert.That(deviceTokenVersionList[1].WorkerId, Is.EqualTo(43));
        Assert.That(deviceTokenVersionList[1].FcmToken, Is.EqualTo(deviceToken.FcmToken));
        Assert.That(deviceTokenVersionList[1].Platform, Is.EqualTo("ios"));
        Assert.That(deviceTokenVersionList[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task DeviceToken_Create_SameTokenDifferentWorkers_DoesSave()
    {
        // Arrange - the (WorkerId, FcmToken) index is unique, but the same
        // token may appear for different workers (shared device).
        var fcmToken = GetRandomStr();

        var deviceTokenOne = new DeviceToken
        {
            WorkerId = 1,
            FcmToken = fcmToken,
            Platform = "android",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        var deviceTokenTwo = new DeviceToken
        {
            WorkerId = 2,
            FcmToken = fcmToken,
            Platform = "android",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await deviceTokenOne.Create(DbContext);
        await deviceTokenTwo.Create(DbContext);

        // Assert
        Assert.That(DbContext.DeviceTokens.AsNoTracking().Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task DeviceToken_Create_DuplicateWorkerAndToken_Throws()
    {
        // Arrange
        var fcmToken = GetRandomStr();

        var deviceTokenOne = new DeviceToken
        {
            WorkerId = 1,
            FcmToken = fcmToken,
            Platform = "android",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        var deviceTokenTwo = new DeviceToken
        {
            WorkerId = 1,
            FcmToken = fcmToken,
            Platform = "android",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await deviceTokenOne.Create(DbContext);

        // Act & Assert
        Assert.ThrowsAsync<DbUpdateException>(async () => await deviceTokenTwo.Create(DbContext));
    }

    [Test]
    public async Task DeviceToken_Delete_DoesDelete()
    {
        // Arrange
        var deviceToken = new DeviceToken
        {
            WorkerId = 42,
            FcmToken = GetRandomStr(),
            Platform = "android",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

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
}
