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
public class AdhocTaskChildrenUTest : DbTestFixture
{
    private static string GetRandomStr() => Guid.NewGuid().ToString();

    private async Task<AdhocTaskEntity> CreateAdhocTask()
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

        var adhocTaskEntity = new AdhocTaskEntity
        {
            Title = GetRandomStr(),
            Description = GetRandomStr(),
            PropertyId = property.Id,
            CreatedByWorkerId = 1,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await adhocTaskEntity.Create(DbContext);

        return adhocTaskEntity;
    }

    private async Task<AdhocTag> CreateAdhocTag()
    {
        var adhocTag = new AdhocTag
        {
            Name = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await adhocTag.Create(DbContext);

        return adhocTag;
    }

    #region AdhocTaskAssignment

    [Test]
    public async Task AdhocTaskAssignment_Create_DoesSave()
    {
        var task = await CreateAdhocTask();

        var assignment = new AdhocTaskAssignment
        {
            AdhocTaskId = task.Id,
            WorkerId = 42,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await assignment.Create(DbContext);

        var list = DbContext.AdhocTaskAssignments.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskAssignmentVersions.AsNoTracking().ToList();

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(1));
        Assert.That(list[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(list[0].WorkerId, Is.EqualTo(42));
        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(list[0].Version, Is.EqualTo(1));

        Assert.That(versionList[0].AdhocTaskAssignmentId, Is.EqualTo(assignment.Id));
        Assert.That(versionList[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(versionList[0].WorkerId, Is.EqualTo(42));
        Assert.That(versionList[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task AdhocTaskAssignment_Update_DoesUpdate()
    {
        var taskOne = await CreateAdhocTask();
        var taskTwo = await CreateAdhocTask();

        var assignment = new AdhocTaskAssignment
        {
            AdhocTaskId = taskOne.Id,
            WorkerId = 1,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await assignment.Create(DbContext);
        var old = DbContext.AdhocTaskAssignments.AsNoTracking().First();

        assignment.AdhocTaskId = taskTwo.Id;
        assignment.WorkerId = 2;
        assignment.UpdatedByUserId = 2;
        await assignment.Update(DbContext);

        var list = DbContext.AdhocTaskAssignments.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskAssignmentVersions.AsNoTracking().ToList();

        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(list[0].AdhocTaskId, Is.EqualTo(taskTwo.Id));
        Assert.That(list[0].WorkerId, Is.EqualTo(2));
        Assert.That(list[0].Version, Is.EqualTo(2));

        Assert.That(versionList[0].AdhocTaskId, Is.EqualTo(old.AdhocTaskId));
        Assert.That(versionList[0].WorkerId, Is.EqualTo(old.WorkerId));
        Assert.That(versionList[1].AdhocTaskId, Is.EqualTo(taskTwo.Id));
        Assert.That(versionList[1].WorkerId, Is.EqualTo(2));
    }

    [Test]
    public async Task AdhocTaskAssignment_Delete_DoesDelete()
    {
        var task = await CreateAdhocTask();

        var assignment = new AdhocTaskAssignment
        {
            AdhocTaskId = task.Id,
            WorkerId = 7,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await assignment.Create(DbContext);

        await assignment.Delete(DbContext);

        var list = DbContext.AdhocTaskAssignments.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskAssignmentVersions.AsNoTracking().ToList();

        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(list[0].Version, Is.EqualTo(2));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(versionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
    }

    #endregion

    #region AdhocTaskAssignmentLog

    [Test]
    public async Task AdhocTaskAssignmentLog_Create_DoesSave()
    {
        var task = await CreateAdhocTask();

        var log = new AdhocTaskAssignmentLog
        {
            AdhocTaskId = task.Id,
            ChangedByWorkerId = 5,
            FromWorkerIdsJson = "[]",
            ToWorkerIdsJson = "[3,7]",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await log.Create(DbContext);

        var list = DbContext.AdhocTaskAssignmentLogs.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskAssignmentLogVersions.AsNoTracking().ToList();

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(1));
        Assert.That(list[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(list[0].ChangedByWorkerId, Is.EqualTo(5));
        Assert.That(list[0].FromWorkerIdsJson, Is.EqualTo("[]"));
        Assert.That(list[0].ToWorkerIdsJson, Is.EqualTo("[3,7]"));

        Assert.That(versionList[0].AdhocTaskAssignmentLogId, Is.EqualTo(log.Id));
        Assert.That(versionList[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(versionList[0].ChangedByWorkerId, Is.EqualTo(5));
        Assert.That(versionList[0].FromWorkerIdsJson, Is.EqualTo("[]"));
        Assert.That(versionList[0].ToWorkerIdsJson, Is.EqualTo("[3,7]"));
    }

    [Test]
    public async Task AdhocTaskAssignmentLog_Update_DoesUpdate()
    {
        var task = await CreateAdhocTask();

        var log = new AdhocTaskAssignmentLog
        {
            AdhocTaskId = task.Id,
            ChangedByWorkerId = 5,
            FromWorkerIdsJson = "[]",
            ToWorkerIdsJson = "[3]",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await log.Create(DbContext);
        var old = DbContext.AdhocTaskAssignmentLogs.AsNoTracking().First();

        log.ChangedByWorkerId = 6;
        log.FromWorkerIdsJson = "[3]";
        log.ToWorkerIdsJson = "[3,7]";
        log.UpdatedByUserId = 2;
        await log.Update(DbContext);

        var list = DbContext.AdhocTaskAssignmentLogs.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskAssignmentLogVersions.AsNoTracking().ToList();

        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(list[0].ChangedByWorkerId, Is.EqualTo(6));
        Assert.That(list[0].FromWorkerIdsJson, Is.EqualTo("[3]"));
        Assert.That(list[0].ToWorkerIdsJson, Is.EqualTo("[3,7]"));

        Assert.That(versionList[0].ChangedByWorkerId, Is.EqualTo(old.ChangedByWorkerId));
        Assert.That(versionList[0].FromWorkerIdsJson, Is.EqualTo(old.FromWorkerIdsJson));
        Assert.That(versionList[0].ToWorkerIdsJson, Is.EqualTo(old.ToWorkerIdsJson));
        Assert.That(versionList[1].ChangedByWorkerId, Is.EqualTo(6));
        Assert.That(versionList[1].FromWorkerIdsJson, Is.EqualTo("[3]"));
        Assert.That(versionList[1].ToWorkerIdsJson, Is.EqualTo("[3,7]"));
    }

    [Test]
    public async Task AdhocTaskAssignmentLog_Delete_DoesDelete()
    {
        var task = await CreateAdhocTask();

        var log = new AdhocTaskAssignmentLog
        {
            AdhocTaskId = task.Id,
            ChangedByWorkerId = 5,
            FromWorkerIdsJson = "[]",
            ToWorkerIdsJson = "[3]",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await log.Create(DbContext);

        await log.Delete(DbContext);

        var list = DbContext.AdhocTaskAssignmentLogs.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskAssignmentLogVersions.AsNoTracking().ToList();

        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(versionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
    }

    #endregion

    #region AdhocTaskComment

    [Test]
    public async Task AdhocTaskComment_Create_DoesSave()
    {
        var task = await CreateAdhocTask();

        var comment = new AdhocTaskComment
        {
            AdhocTaskId = task.Id,
            AuthorWorkerId = 9,
            Text = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await comment.Create(DbContext);

        var list = DbContext.AdhocTaskComments.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskCommentVersions.AsNoTracking().ToList();

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(1));
        Assert.That(list[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(list[0].AuthorWorkerId, Is.EqualTo(9));
        Assert.That(list[0].Text, Is.EqualTo(comment.Text));

        Assert.That(versionList[0].AdhocTaskCommentId, Is.EqualTo(comment.Id));
        Assert.That(versionList[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(versionList[0].AuthorWorkerId, Is.EqualTo(9));
        Assert.That(versionList[0].Text, Is.EqualTo(comment.Text));
    }

    [Test]
    public async Task AdhocTaskComment_Update_DoesUpdate()
    {
        var task = await CreateAdhocTask();

        var comment = new AdhocTaskComment
        {
            AdhocTaskId = task.Id,
            AuthorWorkerId = 9,
            Text = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await comment.Create(DbContext);
        var old = DbContext.AdhocTaskComments.AsNoTracking().First();

        comment.Text = GetRandomStr();
        comment.UpdatedByUserId = 2;
        await comment.Update(DbContext);

        var list = DbContext.AdhocTaskComments.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskCommentVersions.AsNoTracking().ToList();

        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(list[0].Text, Is.EqualTo(comment.Text));
        Assert.That(versionList[0].Text, Is.EqualTo(old.Text));
        Assert.That(versionList[1].Text, Is.EqualTo(comment.Text));
        Assert.That(versionList[1].AuthorWorkerId, Is.EqualTo(9));
    }

    [Test]
    public async Task AdhocTaskComment_Delete_DoesDelete()
    {
        var task = await CreateAdhocTask();

        var comment = new AdhocTaskComment
        {
            AdhocTaskId = task.Id,
            AuthorWorkerId = 9,
            Text = GetRandomStr(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await comment.Create(DbContext);

        await comment.Delete(DbContext);

        var list = DbContext.AdhocTaskComments.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskCommentVersions.AsNoTracking().ToList();

        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(versionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
    }

    #endregion

    #region AdhocTaskPhoto

    [Test]
    public async Task AdhocTaskPhoto_Create_DoesSave()
    {
        var task = await CreateAdhocTask();

        var photo = new AdhocTaskPhoto
        {
            AdhocTaskId = task.Id,
            UploadedDataId = 123,
            ContentType = "image/jpeg",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await photo.Create(DbContext);

        var list = DbContext.AdhocTaskPhotos.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskPhotoVersions.AsNoTracking().ToList();

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(1));
        Assert.That(list[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(list[0].UploadedDataId, Is.EqualTo(123));
        Assert.That(list[0].ContentType, Is.EqualTo("image/jpeg"));

        Assert.That(versionList[0].AdhocTaskPhotoId, Is.EqualTo(photo.Id));
        Assert.That(versionList[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(versionList[0].UploadedDataId, Is.EqualTo(123));
        Assert.That(versionList[0].ContentType, Is.EqualTo("image/jpeg"));
    }

    [Test]
    public async Task AdhocTaskPhoto_Update_DoesUpdate()
    {
        var task = await CreateAdhocTask();

        var photo = new AdhocTaskPhoto
        {
            AdhocTaskId = task.Id,
            UploadedDataId = 123,
            ContentType = "image/jpeg",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await photo.Create(DbContext);
        var old = DbContext.AdhocTaskPhotos.AsNoTracking().First();

        photo.UploadedDataId = 456;
        photo.ContentType = "image/png";
        photo.UpdatedByUserId = 2;
        await photo.Update(DbContext);

        var list = DbContext.AdhocTaskPhotos.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskPhotoVersions.AsNoTracking().ToList();

        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(list[0].UploadedDataId, Is.EqualTo(456));
        Assert.That(list[0].ContentType, Is.EqualTo("image/png"));
        Assert.That(versionList[0].UploadedDataId, Is.EqualTo(old.UploadedDataId));
        Assert.That(versionList[0].ContentType, Is.EqualTo(old.ContentType));
        Assert.That(versionList[1].UploadedDataId, Is.EqualTo(456));
        Assert.That(versionList[1].ContentType, Is.EqualTo("image/png"));
    }

    [Test]
    public async Task AdhocTaskPhoto_Delete_DoesDelete()
    {
        var task = await CreateAdhocTask();

        var photo = new AdhocTaskPhoto
        {
            AdhocTaskId = task.Id,
            UploadedDataId = 123,
            ContentType = "image/jpeg",
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await photo.Create(DbContext);

        await photo.Delete(DbContext);

        var list = DbContext.AdhocTaskPhotos.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskPhotoVersions.AsNoTracking().ToList();

        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(versionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
    }

    #endregion

    #region AdhocTaskTag

    [Test]
    public async Task AdhocTaskTag_Create_DoesSave()
    {
        var task = await CreateAdhocTask();
        var tag = await CreateAdhocTag();

        var taskTag = new AdhocTaskTag
        {
            AdhocTaskId = task.Id,
            AdhocTagId = tag.Id,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };

        await taskTag.Create(DbContext);

        var list = DbContext.AdhocTaskTags.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskTagVersions.AsNoTracking().ToList();

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(versionList.Count, Is.EqualTo(1));
        Assert.That(list[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(list[0].AdhocTagId, Is.EqualTo(tag.Id));

        Assert.That(versionList[0].AdhocTaskTagId, Is.EqualTo(taskTag.Id));
        Assert.That(versionList[0].AdhocTaskId, Is.EqualTo(task.Id));
        Assert.That(versionList[0].AdhocTagId, Is.EqualTo(tag.Id));
    }

    [Test]
    public async Task AdhocTaskTag_Update_DoesUpdate()
    {
        var task = await CreateAdhocTask();
        var tagOne = await CreateAdhocTag();
        var tagTwo = await CreateAdhocTag();

        var taskTag = new AdhocTaskTag
        {
            AdhocTaskId = task.Id,
            AdhocTagId = tagOne.Id,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await taskTag.Create(DbContext);
        var old = DbContext.AdhocTaskTags.AsNoTracking().First();

        taskTag.AdhocTagId = tagTwo.Id;
        taskTag.UpdatedByUserId = 2;
        await taskTag.Update(DbContext);

        var list = DbContext.AdhocTaskTags.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskTagVersions.AsNoTracking().ToList();

        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(list[0].AdhocTagId, Is.EqualTo(tagTwo.Id));
        Assert.That(versionList[0].AdhocTagId, Is.EqualTo(old.AdhocTagId));
        Assert.That(versionList[1].AdhocTagId, Is.EqualTo(tagTwo.Id));
    }

    [Test]
    public async Task AdhocTaskTag_Delete_DoesDelete()
    {
        var task = await CreateAdhocTask();
        var tag = await CreateAdhocTag();

        var taskTag = new AdhocTaskTag
        {
            AdhocTaskId = task.Id,
            AdhocTagId = tag.Id,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        await taskTag.Create(DbContext);

        await taskTag.Delete(DbContext);

        var list = DbContext.AdhocTaskTags.AsNoTracking().ToList();
        var versionList = DbContext.AdhocTaskTagVersions.AsNoTracking().ToList();

        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(versionList.Count, Is.EqualTo(2));
        Assert.That(versionList[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
    }

    #endregion
}
