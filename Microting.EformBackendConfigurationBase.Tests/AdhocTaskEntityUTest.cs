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
public class AdhocTaskEntityUTest : DbTestFixture
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

    private async Task<AdhocArea> CreateArea(int propertyId)
    {
        var adhocArea = new AdhocArea
        {
            PropertyId = propertyId,
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await adhocArea.Create(DbContext);

        return adhocArea;
    }

    // Fully populated, so both the Create and Update tests exercise every
    // column of AdhocTaskEntity/AdhocTaskEntityVersion - the reflection-based
    // MapVersion snapshot silently drops any column that is missing or
    // mistyped on the Version sibling, so every field must be asserted here.
    private async Task<AdhocTaskEntity> CreateFullyPopulatedTask(int propertyId, int? areaId)
    {
        var visibleFrom = new DateTime(2026, 7, 1, 6, 0, 0, DateTimeKind.Utc);
        var deadline = new DateTime(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);
        var completedAt = new DateTime(2026, 7, 5, 8, 0, 0, DateTimeKind.Utc);
        var archivedAt = new DateTime(2026, 7, 6, 9, 0, 0, DateTimeKind.Utc);
        var lastVisibleReminderSentAt = new DateTime(2026, 7, 2, 8, 0, 0, DateTimeKind.Utc);
        var lastDeadlineReminderSentAt = new DateTime(2026, 7, 9, 8, 0, 0, DateTimeKind.Utc);

        var adhocTaskEntity = new AdhocTaskEntity
        {
            Title = GetRandomStr(),
            Description = GetRandomStr(),
            Urgent = true,
            PropertyId = propertyId,
            AreaId = areaId,
            VisibleFrom = visibleFrom,
            Deadline = deadline,
            VisibleReminder = true,
            DeadlineReminder = true,
            DeadlineReminderRepeat = 1,
            VisibleReminderTimeMinutes = 360,
            DeadlineReminderTimeMinutes = 420,
            ExecutionRule = 1,
            CreatedByWorkerId = 11,
            Completed = true,
            CompletedByWorkerId = 22,
            CompletedAt = completedAt,
            Archived = true,
            ArchivedAt = archivedAt,
            LastVisibleReminderSentAt = lastVisibleReminderSentAt,
            LastDeadlineReminderSentAt = lastDeadlineReminderSentAt,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await adhocTaskEntity.Create(DbContext);

        return adhocTaskEntity;
    }

    private static void AssertMatchesEntity(AdhocTaskEntity expected, AdhocTaskEntity actual)
    {
        Assert.That(actual.Title, Is.EqualTo(expected.Title));
        Assert.That(actual.Description, Is.EqualTo(expected.Description));
        Assert.That(actual.Urgent, Is.EqualTo(expected.Urgent));
        Assert.That(actual.PropertyId, Is.EqualTo(expected.PropertyId));
        Assert.That(actual.AreaId, Is.EqualTo(expected.AreaId));
        Assert.That(actual.VisibleFrom, Is.EqualTo(expected.VisibleFrom));
        Assert.That(actual.Deadline, Is.EqualTo(expected.Deadline));
        Assert.That(actual.VisibleReminder, Is.EqualTo(expected.VisibleReminder));
        Assert.That(actual.DeadlineReminder, Is.EqualTo(expected.DeadlineReminder));
        Assert.That(actual.DeadlineReminderRepeat, Is.EqualTo(expected.DeadlineReminderRepeat));
        Assert.That(actual.VisibleReminderTimeMinutes, Is.EqualTo(expected.VisibleReminderTimeMinutes));
        Assert.That(actual.DeadlineReminderTimeMinutes, Is.EqualTo(expected.DeadlineReminderTimeMinutes));
        Assert.That(actual.ExecutionRule, Is.EqualTo(expected.ExecutionRule));
        Assert.That(actual.CreatedByWorkerId, Is.EqualTo(expected.CreatedByWorkerId));
        Assert.That(actual.Completed, Is.EqualTo(expected.Completed));
        Assert.That(actual.CompletedByWorkerId, Is.EqualTo(expected.CompletedByWorkerId));
        Assert.That(actual.CompletedAt, Is.EqualTo(expected.CompletedAt));
        Assert.That(actual.Archived, Is.EqualTo(expected.Archived));
        Assert.That(actual.ArchivedAt, Is.EqualTo(expected.ArchivedAt));
        Assert.That(actual.LastVisibleReminderSentAt, Is.EqualTo(expected.LastVisibleReminderSentAt));
        Assert.That(actual.LastDeadlineReminderSentAt, Is.EqualTo(expected.LastDeadlineReminderSentAt));
    }

    private static void AssertMatchesVersion(AdhocTaskEntity expected, AdhocTaskEntityVersion actual)
    {
        Assert.That(actual.AdhocTaskEntityId, Is.EqualTo(expected.Id));
        Assert.That(actual.Title, Is.EqualTo(expected.Title));
        Assert.That(actual.Description, Is.EqualTo(expected.Description));
        Assert.That(actual.Urgent, Is.EqualTo(expected.Urgent));
        Assert.That(actual.PropertyId, Is.EqualTo(expected.PropertyId));
        Assert.That(actual.AreaId, Is.EqualTo(expected.AreaId));
        Assert.That(actual.VisibleFrom, Is.EqualTo(expected.VisibleFrom));
        Assert.That(actual.Deadline, Is.EqualTo(expected.Deadline));
        Assert.That(actual.VisibleReminder, Is.EqualTo(expected.VisibleReminder));
        Assert.That(actual.DeadlineReminder, Is.EqualTo(expected.DeadlineReminder));
        Assert.That(actual.DeadlineReminderRepeat, Is.EqualTo(expected.DeadlineReminderRepeat));
        Assert.That(actual.VisibleReminderTimeMinutes, Is.EqualTo(expected.VisibleReminderTimeMinutes));
        Assert.That(actual.DeadlineReminderTimeMinutes, Is.EqualTo(expected.DeadlineReminderTimeMinutes));
        Assert.That(actual.ExecutionRule, Is.EqualTo(expected.ExecutionRule));
        Assert.That(actual.CreatedByWorkerId, Is.EqualTo(expected.CreatedByWorkerId));
        Assert.That(actual.Completed, Is.EqualTo(expected.Completed));
        Assert.That(actual.CompletedByWorkerId, Is.EqualTo(expected.CompletedByWorkerId));
        Assert.That(actual.CompletedAt, Is.EqualTo(expected.CompletedAt));
        Assert.That(actual.Archived, Is.EqualTo(expected.Archived));
        Assert.That(actual.ArchivedAt, Is.EqualTo(expected.ArchivedAt));
        Assert.That(actual.LastVisibleReminderSentAt, Is.EqualTo(expected.LastVisibleReminderSentAt));
        Assert.That(actual.LastDeadlineReminderSentAt, Is.EqualTo(expected.LastDeadlineReminderSentAt));
    }

    [Test]
    public async Task AdhocTaskEntity_Create_DoesSave()
    {
        // Arrange
        var property = await CreateProperty();
        var area = await CreateArea(property.Id);

        // Act
        var adhocTaskEntity = await CreateFullyPopulatedTask(property.Id, area.Id);

        var taskList = DbContext.AdhocTasks.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(taskList.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(1));
        Assert.That(taskList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(taskList[0].Version, Is.EqualTo(1));
        AssertMatchesEntity(adhocTaskEntity, taskList[0]);

        Assert.That(versionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(versionList[0].Version, Is.EqualTo(1));
        AssertMatchesVersion(adhocTaskEntity, versionList[0]);
    }

    [Test]
    public async Task AdhocTaskEntity_Create_DoesSave_NullableFieldsNull()
    {
        // Arrange
        var property = await CreateProperty();

        var adhocTaskEntity = new AdhocTaskEntity
        {
            Title = GetRandomStr(),
            Description = GetRandomStr(),
            Urgent = false,
            PropertyId = property.Id,
            AreaId = null,
            VisibleFrom = null,
            Deadline = null,
            VisibleReminder = false,
            DeadlineReminder = false,
            DeadlineReminderRepeat = 0,
            ExecutionRule = 0,
            CreatedByWorkerId = 5,
            Completed = false,
            CompletedByWorkerId = null,
            CompletedAt = null,
            Archived = false,
            ArchivedAt = null,
            LastVisibleReminderSentAt = null,
            LastDeadlineReminderSentAt = null,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        // Act
        await adhocTaskEntity.Create(DbContext);

        var taskList = DbContext.AdhocTasks.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(taskList[0].AreaId, Is.Null);
        Assert.That(taskList[0].CompletedByWorkerId, Is.Null);
        Assert.That(taskList[0].VisibleReminderTimeMinutes, Is.EqualTo(480));
        Assert.That(taskList[0].DeadlineReminderTimeMinutes, Is.EqualTo(480));
        Assert.That(taskList[0].LastVisibleReminderSentAt, Is.Null);
        Assert.That(taskList[0].LastDeadlineReminderSentAt, Is.Null);

        Assert.That(versionList[0].AreaId, Is.Null);
        Assert.That(versionList[0].CompletedByWorkerId, Is.Null);
        Assert.That(versionList[0].VisibleReminderTimeMinutes, Is.EqualTo(480));
        Assert.That(versionList[0].DeadlineReminderTimeMinutes, Is.EqualTo(480));
        Assert.That(versionList[0].LastVisibleReminderSentAt, Is.Null);
        Assert.That(versionList[0].LastDeadlineReminderSentAt, Is.Null);
    }

    [Test]
    public async Task AdhocTaskEntity_Update_DoesUpdate_EveryColumnSurvivesVersionSnapshot()
    {
        // Arrange
        var propertyOne = await CreateProperty();
        var propertyTwo = await CreateProperty();
        var areaOne = await CreateArea(propertyOne.Id);
        var areaTwo = await CreateArea(propertyTwo.Id);

        var adhocTaskEntity = await CreateFullyPopulatedTask(propertyOne.Id, areaOne.Id);
        var beforeUpdate = DbContext.AdhocTasks.AsNoTracking().First();

        // Act - flip every single column to a new value
        adhocTaskEntity.Title = GetRandomStr();
        adhocTaskEntity.Description = GetRandomStr();
        adhocTaskEntity.Urgent = false;
        adhocTaskEntity.PropertyId = propertyTwo.Id;
        adhocTaskEntity.AreaId = areaTwo.Id;
        adhocTaskEntity.VisibleFrom = new DateTime(2026, 8, 1, 6, 0, 0, DateTimeKind.Utc);
        adhocTaskEntity.Deadline = new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc);
        adhocTaskEntity.VisibleReminder = false;
        adhocTaskEntity.DeadlineReminder = false;
        adhocTaskEntity.DeadlineReminderRepeat = 0;
        adhocTaskEntity.VisibleReminderTimeMinutes = 500;
        adhocTaskEntity.DeadlineReminderTimeMinutes = 510;
        adhocTaskEntity.ExecutionRule = 0;
        adhocTaskEntity.CreatedByWorkerId = 33;
        adhocTaskEntity.Completed = false;
        adhocTaskEntity.CompletedByWorkerId = null;
        adhocTaskEntity.CompletedAt = null;
        adhocTaskEntity.Archived = false;
        adhocTaskEntity.ArchivedAt = null;
        adhocTaskEntity.LastVisibleReminderSentAt = new DateTime(2026, 8, 2, 8, 0, 0, DateTimeKind.Utc);
        adhocTaskEntity.LastDeadlineReminderSentAt = new DateTime(2026, 8, 9, 8, 0, 0, DateTimeKind.Utc);
        adhocTaskEntity.UpdatedByUserId = 2;

        await adhocTaskEntity.Update(DbContext);

        var taskList = DbContext.AdhocTasks.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(taskList.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(taskList[0].Id, Is.EqualTo(adhocTaskEntity.Id));
        Assert.That(taskList[0].Version, Is.EqualTo(2));
        AssertMatchesEntity(adhocTaskEntity, taskList[0]);

        // Pre-mutation snapshot preserved at the old values
        Assert.That(versionList[0].AdhocTaskEntityId, Is.EqualTo(adhocTaskEntity.Id));
        Assert.That(versionList[0].Version, Is.EqualTo(1));
        Assert.That(versionList[0].Title, Is.EqualTo(beforeUpdate.Title));
        Assert.That(versionList[0].Description, Is.EqualTo(beforeUpdate.Description));
        Assert.That(versionList[0].Urgent, Is.EqualTo(beforeUpdate.Urgent));
        Assert.That(versionList[0].PropertyId, Is.EqualTo(beforeUpdate.PropertyId));
        Assert.That(versionList[0].AreaId, Is.EqualTo(beforeUpdate.AreaId));
        Assert.That(versionList[0].VisibleFrom, Is.EqualTo(beforeUpdate.VisibleFrom));
        Assert.That(versionList[0].Deadline, Is.EqualTo(beforeUpdate.Deadline));
        Assert.That(versionList[0].VisibleReminder, Is.EqualTo(beforeUpdate.VisibleReminder));
        Assert.That(versionList[0].DeadlineReminder, Is.EqualTo(beforeUpdate.DeadlineReminder));
        Assert.That(versionList[0].DeadlineReminderRepeat, Is.EqualTo(beforeUpdate.DeadlineReminderRepeat));
        Assert.That(versionList[0].VisibleReminderTimeMinutes, Is.EqualTo(beforeUpdate.VisibleReminderTimeMinutes));
        Assert.That(versionList[0].DeadlineReminderTimeMinutes, Is.EqualTo(beforeUpdate.DeadlineReminderTimeMinutes));
        Assert.That(versionList[0].ExecutionRule, Is.EqualTo(beforeUpdate.ExecutionRule));
        Assert.That(versionList[0].CreatedByWorkerId, Is.EqualTo(beforeUpdate.CreatedByWorkerId));
        Assert.That(versionList[0].Completed, Is.EqualTo(beforeUpdate.Completed));
        Assert.That(versionList[0].CompletedByWorkerId, Is.EqualTo(beforeUpdate.CompletedByWorkerId));
        Assert.That(versionList[0].CompletedAt, Is.EqualTo(beforeUpdate.CompletedAt));
        Assert.That(versionList[0].Archived, Is.EqualTo(beforeUpdate.Archived));
        Assert.That(versionList[0].ArchivedAt, Is.EqualTo(beforeUpdate.ArchivedAt));
        Assert.That(versionList[0].LastVisibleReminderSentAt, Is.EqualTo(beforeUpdate.LastVisibleReminderSentAt));
        Assert.That(versionList[0].LastDeadlineReminderSentAt, Is.EqualTo(beforeUpdate.LastDeadlineReminderSentAt));

        // Post-mutation snapshot matches every new value
        Assert.That(versionList[1].AdhocTaskEntityId, Is.EqualTo(adhocTaskEntity.Id));
        Assert.That(versionList[1].Version, Is.EqualTo(2));
        AssertMatchesVersion(adhocTaskEntity, versionList[1]);
    }

    [Test]
    public async Task AdhocTaskEntity_Delete_DoesDelete()
    {
        // Arrange
        var property = await CreateProperty();
        var adhocTaskEntity = await CreateFullyPopulatedTask(property.Id, null);

        // Act
        await adhocTaskEntity.Delete(DbContext);

        var taskList = DbContext.AdhocTasks.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(taskList.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(taskList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(taskList[0].Id, Is.EqualTo(adhocTaskEntity.Id));
        Assert.That(taskList[0].Version, Is.EqualTo(2));

        Assert.That(versionList[0].AdhocTaskEntityId, Is.EqualTo(adhocTaskEntity.Id));
        Assert.That(versionList[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(versionList[0].Version, Is.EqualTo(1));

        Assert.That(versionList[1].AdhocTaskEntityId, Is.EqualTo(adhocTaskEntity.Id));
        Assert.That(versionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(versionList[1].Version, Is.EqualTo(2));
    }
}
