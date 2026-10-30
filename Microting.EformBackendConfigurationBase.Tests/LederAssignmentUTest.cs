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

using System.Linq;
using System.Threading.Tasks;
using eForm.Infrastructure.Constants;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class LederAssignmentUTest : DbTestFixture
{
    private static LederAssignment NewLederAssignment(int eformUserId)
    {
        return new LederAssignment
        {
            EformUserId = eformUserId,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
    }

    [Test]
    public async Task LederAssignment_Create_DoesSaveAndWritesVersionRow()
    {
        var assignment = NewLederAssignment(501);

        await assignment.Create(DbContext);

        var list = DbContext.LederAssignments.AsNoTracking().ToList();
        var versions = DbContext.LederAssignmentVersions.AsNoTracking().ToList();

        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list[0].EformUserId, Is.EqualTo(501));
        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(list[0].Version, Is.EqualTo(1));

        Assert.That(versions, Has.Count.EqualTo(1));
        Assert.That(versions[0].LederAssignmentId, Is.EqualTo(assignment.Id));
        Assert.That(versions[0].EformUserId, Is.EqualTo(501));
        Assert.That(versions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(versions[0].Version, Is.EqualTo(1));
    }

    [Test]
    public async Task LederAssignment_Delete_KeepsRowWithRemovedState()
    {
        var assignment = NewLederAssignment(502);
        await assignment.Create(DbContext);

        await assignment.Delete(DbContext);

        var list = DbContext.LederAssignments.AsNoTracking().ToList();
        var versions = DbContext.LederAssignmentVersions.AsNoTracking().OrderBy(x => x.Version).ToList();

        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list[0].Id, Is.EqualTo(assignment.Id));
        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(list[0].Version, Is.EqualTo(2));

        Assert.That(versions, Has.Count.EqualTo(2));
        Assert.That(versions[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(versions[1].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Removed));
        Assert.That(versions[1].EformUserId, Is.EqualTo(502));
        Assert.That(versions[1].LederAssignmentId, Is.EqualTo(assignment.Id));
    }

    [Test]
    public async Task LederAssignment_Create_SameEformUserId_Throws()
    {
        await NewLederAssignment(503).Create(DbContext);

        // A fresh instance on the same context, as a careless consumer would do.
        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await NewLederAssignment(503).Create(DbContext));
    }

    [Test]
    public async Task LederAssignment_Create_SameEformUserId_ThrowsEvenWhenFirstIsRemoved()
    {
        // The unique index has no WorkflowState filter: a revoked row still
        // blocks a second row. Consumers must upsert (next test).
        var first = NewLederAssignment(504);
        await first.Create(DbContext);
        await first.Delete(DbContext);

        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await NewLederAssignment(504).Create(DbContext));
    }

    [Test]
    public async Task LederAssignment_RevokeThenGrant_ReusesRowAndWritesVersions()
    {
        var assignment = NewLederAssignment(505);
        await assignment.Create(DbContext);
        await assignment.Delete(DbContext);

        // Grant again: flip the same row back to created and Update.
        assignment.WorkflowState = Constants.WorkflowStates.Created;
        assignment.UpdatedByUserId = 2;
        await assignment.Update(DbContext);

        var list = DbContext.LederAssignments.AsNoTracking().ToList();
        var versions = DbContext.LederAssignmentVersions.AsNoTracking().OrderBy(x => x.Version).ToList();

        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list[0].Id, Is.EqualTo(assignment.Id));
        Assert.That(list[0].EformUserId, Is.EqualTo(505));
        Assert.That(list[0].WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
        Assert.That(list[0].UpdatedByUserId, Is.EqualTo(2));
        Assert.That(list[0].Version, Is.EqualTo(3));

        Assert.That(versions, Has.Count.EqualTo(3));
        Assert.That(
            versions.Select(x => x.WorkflowState),
            Is.EqualTo(new[]
            {
                Constants.WorkflowStates.Created,
                Constants.WorkflowStates.Removed,
                Constants.WorkflowStates.Created,
            }));
        Assert.That(versions.Select(x => x.LederAssignmentId), Is.All.EqualTo(assignment.Id));
        Assert.That(versions.Select(x => x.EformUserId), Is.All.EqualTo(505));
    }

    [Test]
    public async Task LederAssignment_DifferentEformUserIds_DoSave()
    {
        await NewLederAssignment(506).Create(DbContext);
        await NewLederAssignment(507).Create(DbContext);

        Assert.That(DbContext.LederAssignments.AsNoTracking().Count(), Is.EqualTo(2));
    }

    private static LederAssignment NewSiteAssignment(int siteId)
    {
        return new LederAssignment
        {
            SiteId = siteId,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
    }

    [Test]
    public async Task LederAssignment_SiteKeyed_SavesWithoutEformUserId_AndTheVersionCarriesSiteId()
    {
        var assignment = NewSiteAssignment(701);

        await assignment.Create(DbContext);

        var row = DbContext.LederAssignments.AsNoTracking().Single();
        Assert.That(row.SiteId, Is.EqualTo(701));
        Assert.That(row.EformUserId, Is.Null);
        var version = DbContext.LederAssignmentVersions.AsNoTracking().Single();
        Assert.That(version.LederAssignmentId, Is.EqualTo(assignment.Id));
        Assert.That(version.SiteId, Is.EqualTo(701));
        Assert.That(version.EformUserId, Is.Null);
    }

    [Test]
    public async Task LederAssignment_Create_SameSiteId_Throws()
    {
        await NewSiteAssignment(702).Create(DbContext);

        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await NewSiteAssignment(702).Create(DbContext));
    }

    [Test]
    public async Task LederAssignment_Create_SameSiteId_ThrowsEvenWhenFirstIsRemoved()
    {
        // No WorkflowState filter on the SiteId index either: grant and revoke flip one row per site.
        var first = NewSiteAssignment(703);
        await first.Create(DbContext);
        await first.Delete(DbContext);

        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await NewSiteAssignment(703).Create(DbContext));
    }

    [Test]
    public async Task LederAssignment_ManySiteRows_WithoutEformUserId_DoSave()
    {
        // EformUserId is NULL on every site-keyed row: a unique index never treats two NULLs as equal.
        await NewSiteAssignment(704).Create(DbContext);
        await NewSiteAssignment(705).Create(DbContext);
        await NewSiteAssignment(706).Create(DbContext);

        Assert.That(DbContext.LederAssignments.AsNoTracking().Count(x => x.EformUserId == null), Is.EqualTo(3));
    }

    [Test]
    public async Task LederAssignment_LegacyLoginRow_AndSiteRows_Coexist()
    {
        await NewLederAssignment(707).Create(DbContext);
        await NewSiteAssignment(708).Create(DbContext);

        var rows = DbContext.LederAssignments.AsNoTracking().OrderBy(x => x.Id).ToList();
        Assert.That(rows.Select(x => (x.EformUserId, x.SiteId)),
            Is.EqualTo(new (int?, int?)[] { (707, null), (null, 708) }));
    }

    [Test]
    public async Task LederAssignment_SiteRow_RevokeThenGrant_ReusesRowAndWritesVersions()
    {
        var assignment = NewSiteAssignment(709);
        await assignment.Create(DbContext);
        await assignment.Delete(DbContext);

        assignment.WorkflowState = Constants.WorkflowStates.Created;
        assignment.UpdatedByUserId = 2;
        await assignment.Update(DbContext);

        var row = DbContext.LederAssignments.AsNoTracking().Single();
        Assert.That((row.Id, row.SiteId, row.WorkflowState, row.Version),
            Is.EqualTo((assignment.Id, (int?)709, Constants.WorkflowStates.Created, 3)));
        var versions = DbContext.LederAssignmentVersions.AsNoTracking().OrderBy(x => x.Version).ToList();
        Assert.That(versions.Select(x => x.SiteId), Is.All.EqualTo(709));
        Assert.That(versions, Has.Count.EqualTo(3));
    }
}
