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
public class PropertiesUTest : DbTestFixture
{
    private static string GetRandomStr() => Guid.NewGuid().ToString();
    [Test]
    public async Task Properties_Save_DoesSave()
    {
        // Arrange
        var properties = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await properties.Create(DbContext);

        var propertiesList = DbContext.Properties.AsNoTracking().ToList();
        var propertiesListVersions = DbContext.PropertieVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(propertiesList.Count, Is.EqualTo(1));
        Assert.That(propertiesListVersions.Count, Is.EqualTo(1));
        Assert.That(propertiesList[0].Address, Is.EqualTo(properties.Address));
        Assert.That(propertiesList[0].CHR, Is.EqualTo(properties.CHR));
        Assert.That(propertiesList[0].Name, Is.EqualTo(properties.Name));
        Assert.That(propertiesList[0].CreatedByUserId, Is.EqualTo(properties.CreatedByUserId));
        Assert.That(propertiesList[0].UpdatedByUserId, Is.EqualTo(properties.UpdatedByUserId));
        Assert.That(propertiesList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertiesList[0].Version, Is.EqualTo(1));

        Assert.That(propertiesListVersions[0].Address, Is.EqualTo(properties.Address));
        Assert.That(propertiesListVersions[0].CHR, Is.EqualTo(properties.CHR));
        Assert.That(propertiesListVersions[0].Name, Is.EqualTo(properties.Name));
        Assert.That(propertiesListVersions[0].CreatedByUserId, Is.EqualTo(properties.CreatedByUserId));
        Assert.That(propertiesListVersions[0].UpdatedByUserId, Is.EqualTo(properties.UpdatedByUserId));
        Assert.That(propertiesListVersions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertiesListVersions[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task Properties_Update_DoesUpdate()
    {
        // Arrange
        var properties = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await properties.Create(DbContext);
        var propertiesOld = DbContext.Properties.AsNoTracking().First();
        // Act

        properties.Address = GetRandomStr();
        properties.CHR = GetRandomStr();
        properties.Name = GetRandomStr();
        properties.UpdatedByUserId = 2;
        await properties.Update(DbContext);

        var propertiesList = DbContext.Properties.AsNoTracking().ToList();
        var propertiesListVersions = DbContext.PropertieVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(propertiesList.Count, Is.EqualTo(1));
        Assert.That(propertiesListVersions.Count, Is.EqualTo(2));
        Assert.That(propertiesList[0].Address, Is.EqualTo(properties.Address));
        Assert.That(propertiesList[0].CHR, Is.EqualTo(properties.CHR));
        Assert.That(propertiesList[0].Name, Is.EqualTo(properties.Name));
        Assert.That(propertiesList[0].CreatedByUserId, Is.EqualTo(properties.CreatedByUserId));
        Assert.That(propertiesList[0].UpdatedByUserId, Is.EqualTo(properties.UpdatedByUserId));
        Assert.That(propertiesList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertiesList[0].Id, Is.EqualTo(properties.Id));
        Assert.That(propertiesList[0].Version, Is.EqualTo(2));

        Assert.That(propertiesListVersions[0].Address, Is.EqualTo(propertiesOld.Address));
        Assert.That(propertiesListVersions[0].CHR, Is.EqualTo(propertiesOld.CHR));
        Assert.That(propertiesListVersions[0].Name, Is.EqualTo(propertiesOld.Name));
        Assert.That(propertiesListVersions[0].CreatedByUserId, Is.EqualTo(propertiesOld.CreatedByUserId));
        Assert.That(propertiesListVersions[0].UpdatedByUserId, Is.EqualTo(propertiesOld.UpdatedByUserId));
        Assert.That(propertiesListVersions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertiesListVersions[0].PropertyId, Is.EqualTo(properties.Id));
        Assert.That(propertiesListVersions[0].Version, Is.EqualTo(1));

        Assert.That(propertiesListVersions[1].Address, Is.EqualTo(properties.Address));
        Assert.That(propertiesListVersions[1].CHR, Is.EqualTo(properties.CHR));
        Assert.That(propertiesListVersions[1].Name, Is.EqualTo(properties.Name));
        Assert.That(propertiesListVersions[1].CreatedByUserId, Is.EqualTo(properties.CreatedByUserId));
        Assert.That(propertiesListVersions[1].UpdatedByUserId, Is.EqualTo(properties.UpdatedByUserId));
        Assert.That(propertiesListVersions[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertiesListVersions[0].PropertyId, Is.EqualTo(properties.Id));
        Assert.That(propertiesListVersions[1].Version, Is.EqualTo(2));
    }

    [Test]
    public async Task Properties_Delete_DoesDelete()
    {
        // Arrange
        var properties = new Property
        {
            Address = GetRandomStr(),
            CHR = GetRandomStr(),
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await properties.Create(DbContext);
        // Act

        await properties.Delete(DbContext);

        var propertiesList = DbContext.Properties.AsNoTracking().ToList();
        var propertiesListVersions = DbContext.PropertieVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(propertiesList.Count, Is.EqualTo(1));
        Assert.That(propertiesListVersions.Count, Is.EqualTo(2));
        Assert.That(propertiesList[0].Address, Is.EqualTo(properties.Address));
        Assert.That(propertiesList[0].CHR, Is.EqualTo(properties.CHR));
        Assert.That(propertiesList[0].Name, Is.EqualTo(properties.Name));
        Assert.That(propertiesList[0].CreatedByUserId, Is.EqualTo(properties.CreatedByUserId));
        Assert.That(propertiesList[0].UpdatedByUserId, Is.EqualTo(properties.UpdatedByUserId));
        Assert.That(propertiesList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(propertiesList[0].Id, Is.EqualTo(properties.Id));
        Assert.That(propertiesList[0].Version, Is.EqualTo(2));

        Assert.That(propertiesListVersions[0].Address, Is.EqualTo(properties.Address));
        Assert.That(propertiesListVersions[0].CHR, Is.EqualTo(properties.CHR));
        Assert.That(propertiesListVersions[0].Name, Is.EqualTo(properties.Name));
        Assert.That(propertiesListVersions[0].CreatedByUserId, Is.EqualTo(properties.CreatedByUserId));
        Assert.That(propertiesListVersions[0].UpdatedByUserId, Is.EqualTo(properties.UpdatedByUserId));
        Assert.That(propertiesListVersions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(propertiesListVersions[0].Version, Is.EqualTo(1));

        Assert.That(propertiesListVersions[1].Address, Is.EqualTo(properties.Address));
        Assert.That(propertiesListVersions[1].CHR, Is.EqualTo(properties.CHR));
        Assert.That(propertiesListVersions[1].Name, Is.EqualTo(properties.Name));
        Assert.That(propertiesListVersions[1].CreatedByUserId, Is.EqualTo(properties.CreatedByUserId));
        Assert.That(propertiesListVersions[1].UpdatedByUserId, Is.EqualTo(properties.UpdatedByUserId));
        Assert.That(propertiesListVersions[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(propertiesListVersions[1].Version, Is.EqualTo(2));
    }
}