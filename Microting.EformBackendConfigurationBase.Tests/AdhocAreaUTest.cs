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
public class AdhocAreaUTest : DbTestFixture
{
    private static string GetRandomStr() => Guid.NewGuid().ToString();

    private async Task<Property> CreateProperty()
    {
        var property = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await property.Create(DbContext);

        return property;
    }

    [Test]
    public async Task AdhocArea_Create_DoesSave()
    {
        // Arrange
        var property = await CreateProperty();

        var adhocArea = new AdhocArea
        {
            PropertyId = property.Id,
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await adhocArea.Create(DbContext);

        var adhocAreaList = DbContext.AdhocAreas.AsNoTracking().ToList();
        var adhocAreaVersionList = DbContext.AdhocAreaVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(adhocAreaList.Count, Is.EqualTo(1));
        Assert.That(adhocAreaVersionList.Count, Is.EqualTo(1));
        Assert.That(adhocAreaList[0].PropertyId, Is.EqualTo(property.Id));
        Assert.That(adhocAreaList[0].Name, Is.EqualTo(adhocArea.Name));
        Assert.That(adhocAreaList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocAreaList[0].Version, Is.EqualTo(1));

        Assert.That(adhocAreaVersionList[0].AdhocAreaId, Is.EqualTo(adhocArea.Id));
        Assert.That(adhocAreaVersionList[0].PropertyId, Is.EqualTo(property.Id));
        Assert.That(adhocAreaVersionList[0].Name, Is.EqualTo(adhocArea.Name));
        Assert.That(adhocAreaVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocAreaVersionList[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task AdhocArea_Update_DoesUpdate()
    {
        // Arrange
        var propertyOne = await CreateProperty();
        var propertyTwo = await CreateProperty();

        var adhocArea = new AdhocArea
        {
            PropertyId = propertyOne.Id,
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await adhocArea.Create(DbContext);
        var adhocAreaOld = DbContext.AdhocAreas.AsNoTracking().First();

        // Act
        adhocArea.PropertyId = propertyTwo.Id;
        adhocArea.Name = GetRandomStr();
        adhocArea.UpdatedByUserId = 2;

        await adhocArea.Update(DbContext);

        var adhocAreaList = DbContext.AdhocAreas.AsNoTracking().ToList();
        var adhocAreaVersionList = DbContext.AdhocAreaVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(adhocAreaList.Count, Is.EqualTo(1));
        Assert.That(adhocAreaVersionList.Count, Is.EqualTo(2));
        Assert.That(adhocAreaList[0].PropertyId, Is.EqualTo(propertyTwo.Id));
        Assert.That(adhocAreaList[0].Name, Is.EqualTo(adhocArea.Name));
        Assert.That(adhocAreaList[0].UpdatedByUserId, Is.EqualTo(2));
        Assert.That(adhocAreaList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocAreaList[0].Id, Is.EqualTo(adhocArea.Id));
        Assert.That(adhocAreaList[0].Version, Is.EqualTo(2));

        Assert.That(adhocAreaVersionList[0].AdhocAreaId, Is.EqualTo(adhocArea.Id));
        Assert.That(adhocAreaVersionList[0].PropertyId, Is.EqualTo(adhocAreaOld.PropertyId));
        Assert.That(adhocAreaVersionList[0].Name, Is.EqualTo(adhocAreaOld.Name));
        Assert.That(adhocAreaVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocAreaVersionList[0].Version, Is.EqualTo(1));

        Assert.That(adhocAreaVersionList[1].AdhocAreaId, Is.EqualTo(adhocArea.Id));
        Assert.That(adhocAreaVersionList[1].PropertyId, Is.EqualTo(propertyTwo.Id));
        Assert.That(adhocAreaVersionList[1].Name, Is.EqualTo(adhocArea.Name));
        Assert.That(adhocAreaVersionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocAreaVersionList[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task AdhocArea_Delete_DoesDelete()
    {
        // Arrange
        var property = await CreateProperty();

        var adhocArea = new AdhocArea
        {
            PropertyId = property.Id,
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await adhocArea.Create(DbContext);

        // Act
        await adhocArea.Delete(DbContext);

        var adhocAreaList = DbContext.AdhocAreas.AsNoTracking().ToList();
        var adhocAreaVersionList = DbContext.AdhocAreaVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(adhocAreaList.Count, Is.EqualTo(1));
        Assert.That(adhocAreaVersionList.Count, Is.EqualTo(2));
        Assert.That(adhocAreaList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(adhocAreaList[0].Id, Is.EqualTo(adhocArea.Id));
        Assert.That(adhocAreaList[0].Version, Is.EqualTo(2));

        Assert.That(adhocAreaVersionList[0].AdhocAreaId, Is.EqualTo(adhocArea.Id));
        Assert.That(adhocAreaVersionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(adhocAreaVersionList[0].Version, Is.EqualTo(1));

        Assert.That(adhocAreaVersionList[1].AdhocAreaId, Is.EqualTo(adhocArea.Id));
        Assert.That(adhocAreaVersionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(adhocAreaVersionList[1].Version, Is.EqualTo(2));
    }
}
