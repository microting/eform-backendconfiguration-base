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
public class PropertyWorkersUTest : DbTestFixture
{
    private static string GetRandomStr() => Guid.NewGuid().ToString();
    [Test]
    public async Task PropertyWorkers_Save_DoesSave()
    {
        var properties = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
            
        await properties.Create(DbContext);

        // Arrange
        var propertyWorkers = new PropertyWorker
        {
            WorkerId = 1,
            PropertyId = properties.Id,
            UpdatedByUserId = 1,
            CreatedByUserId = 1,
        };

        // Act
        await propertyWorkers.Create(DbContext);
            
        var propertyWorkersList = DbContext.PropertyWorkers
            .AsNoTracking()
            .ToList();
            
        var propertyWorkersListVersions = DbContext.PropertyWorkerVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(propertyWorkersList.Count, Is.EqualTo(1));
        Assert.That(propertyWorkersListVersions.Count, Is.EqualTo(1));
        Assert.That(propertyWorkersList[0].CreatedByUserId, Is.EqualTo(propertyWorkers.CreatedByUserId));
        Assert.That(propertyWorkersList[0].UpdatedByUserId, Is.EqualTo(propertyWorkers.UpdatedByUserId));
        Assert.That(propertyWorkersList[0].WorkerId, Is.EqualTo(propertyWorkers.WorkerId));
        Assert.That(propertyWorkersList[0].PropertyId, Is.EqualTo(propertyWorkers.PropertyId));
        Assert.That(propertyWorkersList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertyWorkersList[0].Id, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersList[0].Version, Is.EqualTo(1));

        // versions
        Assert.That(propertyWorkersListVersions[0].CreatedByUserId, Is.EqualTo(propertyWorkers.CreatedByUserId));
        Assert.That(propertyWorkersListVersions[0].UpdatedByUserId, Is.EqualTo(propertyWorkers.UpdatedByUserId));
        Assert.That(propertyWorkersListVersions[0].WorkerId, Is.EqualTo(propertyWorkers.WorkerId));
        Assert.That(propertyWorkersListVersions[0].PropertyId, Is.EqualTo(propertyWorkers.PropertyId));
        Assert.That(propertyWorkersListVersions[0].PropertyWorkerId, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersListVersions[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task PropertyWorkers_Update_DoesUpdate()
    {
        var oneProperties = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        var twoProperties = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await oneProperties.Create(DbContext);
        await twoProperties.Create(DbContext);

        // Arrange
        var propertyWorkers = new PropertyWorker
        {
            WorkerId = 1,
            PropertyId = oneProperties.Id,
            UpdatedByUserId = 1,
            CreatedByUserId = 1,
        };

        // Act
        await propertyWorkers.Create(DbContext);

        var propertyWorkerOld = DbContext.PropertyWorkers.AsNoTracking().First();

        propertyWorkers.PropertyId = twoProperties.Id;
        propertyWorkers.WorkerId = 2;

        await propertyWorkers.Update(DbContext);

        var propertyWorkersList = DbContext.PropertyWorkers.AsNoTracking().ToList();
        var propertyWorkersListVersions = DbContext.PropertyWorkerVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(propertyWorkersList.Count, Is.EqualTo(1));
        Assert.That(propertyWorkersListVersions.Count, Is.EqualTo(2));
        Assert.That(propertyWorkersList[0].WorkerId, Is.EqualTo(propertyWorkers.WorkerId));
        Assert.That(propertyWorkersList[0].PropertyId, Is.EqualTo(propertyWorkers.PropertyId));
        Assert.That(propertyWorkersList[0].CreatedByUserId, Is.EqualTo(propertyWorkers.CreatedByUserId));
        Assert.That(propertyWorkersList[0].UpdatedByUserId, Is.EqualTo(propertyWorkers.UpdatedByUserId));
        Assert.That(propertyWorkersList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertyWorkersList[0].Id, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersList[0].Version, Is.EqualTo(2));

        // versions
        Assert.That(propertyWorkersListVersions[0].WorkerId, Is.EqualTo(propertyWorkerOld.WorkerId));
        Assert.That(propertyWorkersListVersions[0].PropertyId, Is.EqualTo(propertyWorkerOld.PropertyId));
        Assert.That(propertyWorkersListVersions[0].CreatedByUserId, Is.EqualTo(propertyWorkerOld.CreatedByUserId));
        Assert.That(propertyWorkersListVersions[0].UpdatedByUserId, Is.EqualTo(propertyWorkerOld.UpdatedByUserId));
        Assert.That(propertyWorkersListVersions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertyWorkersListVersions[0].PropertyWorkerId, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersListVersions[0].Version, Is.EqualTo(1));

        Assert.That(propertyWorkersListVersions[1].WorkerId, Is.EqualTo(propertyWorkers.WorkerId));
        Assert.That(propertyWorkersListVersions[1].PropertyId, Is.EqualTo(propertyWorkers.PropertyId));
        Assert.That(propertyWorkersListVersions[1].CreatedByUserId, Is.EqualTo(propertyWorkers.CreatedByUserId));
        Assert.That(propertyWorkersListVersions[1].UpdatedByUserId, Is.EqualTo(propertyWorkers.UpdatedByUserId));
        Assert.That(propertyWorkersListVersions[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertyWorkersListVersions[1].PropertyWorkerId, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersListVersions[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task PropertyWorkers_Delete_DoesDelete()
    {
        var oneProperties = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await oneProperties.Create(DbContext);

        // Arrange
        var propertyWorkers = new PropertyWorker
        {
            WorkerId = 1,
            PropertyId = oneProperties.Id,
            UpdatedByUserId = 1,
            CreatedByUserId = 1,
        };

        // Act
        await propertyWorkers.Create(DbContext);
        await propertyWorkers.Delete(DbContext);

        var propertyWorkersList = DbContext.PropertyWorkers.AsNoTracking().ToList();
        var propertyWorkersListVersions = DbContext.PropertyWorkerVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(propertyWorkersList.Count, Is.EqualTo(1));
        Assert.That(propertyWorkersListVersions.Count, Is.EqualTo(2));
        Assert.That(propertyWorkersList[0].WorkerId, Is.EqualTo(propertyWorkers.WorkerId));
        Assert.That(propertyWorkersList[0].PropertyId, Is.EqualTo(propertyWorkers.PropertyId));
        Assert.That(propertyWorkersList[0].CreatedByUserId, Is.EqualTo(propertyWorkers.CreatedByUserId));
        Assert.That(propertyWorkersList[0].UpdatedByUserId, Is.EqualTo(propertyWorkers.UpdatedByUserId));
        Assert.That(propertyWorkersList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(propertyWorkersList[0].Id, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersList[0].Version, Is.EqualTo(2));

        // versions
        Assert.That(propertyWorkersListVersions[0].WorkerId, Is.EqualTo(propertyWorkers.WorkerId));
        Assert.That(propertyWorkersListVersions[0].PropertyId, Is.EqualTo(propertyWorkers.PropertyId));
        Assert.That(propertyWorkersListVersions[0].CreatedByUserId, Is.EqualTo(propertyWorkers.CreatedByUserId));
        Assert.That(propertyWorkersListVersions[0].UpdatedByUserId, Is.EqualTo(propertyWorkers.UpdatedByUserId));
        Assert.That(propertyWorkersListVersions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertyWorkersListVersions[0].PropertyWorkerId, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersListVersions[0].Version, Is.EqualTo(1));

        Assert.That(propertyWorkersListVersions[1].WorkerId, Is.EqualTo(propertyWorkers.WorkerId));
        Assert.That(propertyWorkersListVersions[1].PropertyId, Is.EqualTo(propertyWorkers.PropertyId));
        Assert.That(propertyWorkersListVersions[1].CreatedByUserId, Is.EqualTo(propertyWorkers.CreatedByUserId));
        Assert.That(propertyWorkersListVersions[1].UpdatedByUserId, Is.EqualTo(propertyWorkers.UpdatedByUserId));
        Assert.That(propertyWorkersListVersions[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(propertyWorkersListVersions[1].PropertyWorkerId, Is.EqualTo(propertyWorkers.Id));
        Assert.That(propertyWorkersListVersions[1].Version, Is.EqualTo(2));
    }
}