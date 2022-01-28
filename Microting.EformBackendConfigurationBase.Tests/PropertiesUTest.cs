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

namespace Microting.EformBackendConfigurationBase.Tests
{
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
            Assert.AreEqual(1, propertiesList.Count);
            Assert.AreEqual(1, propertiesListVersions.Count);
            Assert.AreEqual(properties.Address, propertiesList[0].Address);
            Assert.AreEqual(properties.CHR, propertiesList[0].CHR);
            Assert.AreEqual(properties.Name, propertiesList[0].Name);
            Assert.AreEqual(properties.CreatedByUserId, propertiesList[0].CreatedByUserId);
            Assert.AreEqual(properties.UpdatedByUserId, propertiesList[0].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Created, propertiesList[0].WorkflowState);
            Assert.AreEqual(1, propertiesList[0].Version);

            Assert.AreEqual(properties.Address, propertiesListVersions[0].Address);
            Assert.AreEqual(properties.CHR, propertiesListVersions[0].CHR);
            Assert.AreEqual(properties.Name, propertiesListVersions[0].Name);
            Assert.AreEqual(properties.CreatedByUserId, propertiesListVersions[0].CreatedByUserId);
            Assert.AreEqual(properties.UpdatedByUserId, propertiesListVersions[0].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Created, propertiesListVersions[0].WorkflowState);
            Assert.AreEqual(1, propertiesListVersions[0].Version);
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
            Assert.AreEqual(1, propertiesList.Count);
            Assert.AreEqual(2, propertiesListVersions.Count);
            Assert.AreEqual(properties.Address, propertiesList[0].Address);
            Assert.AreEqual(properties.CHR, propertiesList[0].CHR);
            Assert.AreEqual(properties.Name, propertiesList[0].Name);
            Assert.AreEqual(properties.CreatedByUserId, propertiesList[0].CreatedByUserId);
            Assert.AreEqual(properties.UpdatedByUserId, propertiesList[0].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Created, propertiesList[0].WorkflowState);
            Assert.AreEqual(properties.Id, propertiesList[0].Id);
            Assert.AreEqual(2, propertiesList[0].Version);

            Assert.AreEqual(propertiesOld.Address, propertiesListVersions[0].Address);
            Assert.AreEqual(propertiesOld.CHR, propertiesListVersions[0].CHR);
            Assert.AreEqual(propertiesOld.Name, propertiesListVersions[0].Name);
            Assert.AreEqual(propertiesOld.CreatedByUserId, propertiesListVersions[0].CreatedByUserId);
            Assert.AreEqual(propertiesOld.UpdatedByUserId, propertiesListVersions[0].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Created, propertiesListVersions[0].WorkflowState);
            Assert.AreEqual(properties.Id, propertiesListVersions[0].PropertyId);
            Assert.AreEqual(1, propertiesListVersions[0].Version);

            Assert.AreEqual(properties.Address, propertiesListVersions[1].Address);
            Assert.AreEqual(properties.CHR, propertiesListVersions[1].CHR);
            Assert.AreEqual(properties.Name, propertiesListVersions[1].Name);
            Assert.AreEqual(properties.CreatedByUserId, propertiesListVersions[1].CreatedByUserId);
            Assert.AreEqual(properties.UpdatedByUserId, propertiesListVersions[1].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Created, propertiesListVersions[1].WorkflowState);
            Assert.AreEqual(properties.Id, propertiesListVersions[0].PropertyId);
            Assert.AreEqual(2, propertiesListVersions[1].Version);
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
            Assert.AreEqual(1, propertiesList.Count);
            Assert.AreEqual(2, propertiesListVersions.Count);
            Assert.AreEqual(properties.Address, propertiesList[0].Address);
            Assert.AreEqual(properties.CHR, propertiesList[0].CHR);
            Assert.AreEqual(properties.Name, propertiesList[0].Name);
            Assert.AreEqual(properties.CreatedByUserId, propertiesList[0].CreatedByUserId);
            Assert.AreEqual(properties.UpdatedByUserId, propertiesList[0].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Removed, propertiesList[0].WorkflowState);
            Assert.AreEqual(properties.Id, propertiesList[0].Id);
            Assert.AreEqual(2, propertiesList[0].Version);

            Assert.AreEqual(properties.Address, propertiesListVersions[0].Address);
            Assert.AreEqual(properties.CHR, propertiesListVersions[0].CHR);
            Assert.AreEqual(properties.Name, propertiesListVersions[0].Name);
            Assert.AreEqual(properties.CreatedByUserId, propertiesListVersions[0].CreatedByUserId);
            Assert.AreEqual(properties.UpdatedByUserId, propertiesListVersions[0].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Created, propertiesListVersions[0].WorkflowState);
            Assert.AreEqual(1, propertiesListVersions[0].Version);

            Assert.AreEqual(properties.Address, propertiesListVersions[1].Address);
            Assert.AreEqual(properties.CHR, propertiesListVersions[1].CHR);
            Assert.AreEqual(properties.Name, propertiesListVersions[1].Name);
            Assert.AreEqual(properties.CreatedByUserId, propertiesListVersions[1].CreatedByUserId);
            Assert.AreEqual(properties.UpdatedByUserId, propertiesListVersions[1].UpdatedByUserId);
            Assert.AreEqual(Constants.WorkflowStates.Removed, propertiesListVersions[1].WorkflowState);
            Assert.AreEqual(2, propertiesListVersions[1].Version);
        }
    }
}