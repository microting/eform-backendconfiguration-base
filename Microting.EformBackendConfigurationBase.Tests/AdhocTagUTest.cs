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
public class AdhocTagUTest : DbTestFixture
{
    private static string GetRandomStr() => Guid.NewGuid().ToString();

    [Test]
    public async Task AdhocTag_Create_DoesSave_GlobalTag()
    {
        // Arrange
        var adhocTag = new AdhocTag
        {
            Name = GetRandomStr(),
            OwnerWorkerId = null,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await adhocTag.Create(DbContext);

        var adhocTagList = DbContext.AdhocTags.AsNoTracking().ToList();
        var adhocTagVersionList = DbContext.AdhocTagVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(adhocTagList.Count, Is.EqualTo(1));
        Assert.That(adhocTagVersionList.Count, Is.EqualTo(1));
        Assert.That(adhocTagList[0].Name, Is.EqualTo(adhocTag.Name));
        Assert.That(adhocTagList[0].OwnerWorkerId, Is.Null);
        Assert.That(adhocTagList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocTagList[0].Version, Is.EqualTo(1));

        Assert.That(adhocTagVersionList[0].AdhocTagId, Is.EqualTo(adhocTag.Id));
        Assert.That(adhocTagVersionList[0].Name, Is.EqualTo(adhocTag.Name));
        Assert.That(adhocTagVersionList[0].OwnerWorkerId, Is.Null);
        Assert.That(adhocTagVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocTagVersionList[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task AdhocTag_Create_DoesSave_UserOwnedTag()
    {
        // Arrange
        var adhocTag = new AdhocTag
        {
            Name = GetRandomStr(),
            OwnerWorkerId = 42,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await adhocTag.Create(DbContext);

        var adhocTagList = DbContext.AdhocTags.AsNoTracking().ToList();
        var adhocTagVersionList = DbContext.AdhocTagVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(adhocTagList[0].OwnerWorkerId, Is.EqualTo(42));
        Assert.That(adhocTagVersionList[0].OwnerWorkerId, Is.EqualTo(42));
    }

    [Test]
    public async Task AdhocTag_Update_DoesUpdate()
    {
        // Arrange
        var adhocTag = new AdhocTag
        {
            Name = GetRandomStr(),
            OwnerWorkerId = 42,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await adhocTag.Create(DbContext);
        var adhocTagOld = DbContext.AdhocTags.AsNoTracking().First();

        // Act
        adhocTag.Name = GetRandomStr();
        adhocTag.OwnerWorkerId = null;
        adhocTag.UpdatedByUserId = 2;

        await adhocTag.Update(DbContext);

        var adhocTagList = DbContext.AdhocTags.AsNoTracking().ToList();
        var adhocTagVersionList = DbContext.AdhocTagVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(adhocTagList.Count, Is.EqualTo(1));
        Assert.That(adhocTagVersionList.Count, Is.EqualTo(2));
        Assert.That(adhocTagList[0].Name, Is.EqualTo(adhocTag.Name));
        Assert.That(adhocTagList[0].OwnerWorkerId, Is.Null);
        Assert.That(adhocTagList[0].UpdatedByUserId, Is.EqualTo(2));
        Assert.That(adhocTagList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocTagList[0].Id, Is.EqualTo(adhocTag.Id));
        Assert.That(adhocTagList[0].Version, Is.EqualTo(2));

        Assert.That(adhocTagVersionList[0].AdhocTagId, Is.EqualTo(adhocTag.Id));
        Assert.That(adhocTagVersionList[0].Name, Is.EqualTo(adhocTagOld.Name));
        Assert.That(adhocTagVersionList[0].OwnerWorkerId, Is.EqualTo(42));
        Assert.That(adhocTagVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocTagVersionList[0].Version, Is.EqualTo(1));

        Assert.That(adhocTagVersionList[1].AdhocTagId, Is.EqualTo(adhocTag.Id));
        Assert.That(adhocTagVersionList[1].Name, Is.EqualTo(adhocTag.Name));
        Assert.That(adhocTagVersionList[1].OwnerWorkerId, Is.Null);
        Assert.That(adhocTagVersionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocTagVersionList[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task AdhocTag_Delete_DoesDelete()
    {
        // Arrange
        var adhocTag = new AdhocTag
        {
            Name = GetRandomStr(),
            OwnerWorkerId = 7,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await adhocTag.Create(DbContext);

        // Act
        await adhocTag.Delete(DbContext);

        var adhocTagList = DbContext.AdhocTags.AsNoTracking().ToList();
        var adhocTagVersionList = DbContext.AdhocTagVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(adhocTagList.Count, Is.EqualTo(1));
        Assert.That(adhocTagVersionList.Count, Is.EqualTo(2));
        Assert.That(adhocTagList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(adhocTagList[0].Id, Is.EqualTo(adhocTag.Id));
        Assert.That(adhocTagList[0].Version, Is.EqualTo(2));

        Assert.That(adhocTagVersionList[0].AdhocTagId, Is.EqualTo(adhocTag.Id));
        Assert.That(adhocTagVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocTagVersionList[0].Version, Is.EqualTo(1));

        Assert.That(adhocTagVersionList[1].AdhocTagId, Is.EqualTo(adhocTag.Id));
        Assert.That(adhocTagVersionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(adhocTagVersionList[1].Version, Is.EqualTo(2));
    }
}
