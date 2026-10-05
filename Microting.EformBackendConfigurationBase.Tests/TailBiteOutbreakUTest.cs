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
using System.Threading.Tasks;
using Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class TailBiteOutbreakUTest : DbTestFixture
{
    private static TailBiteOutbreak Open(int locationId) => new()
    {
        PropertyId = 1, LocationId = locationId, RuleId = 1, RuleVersion = 1,
        OpenedAt = new DateTime(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc), OpenedByRegistrationId = 1
    };

    [Test]
    public async Task TwoOpenOutbreaks_SameLocation_Rejected()
    {
        await Open(10).Create(DbContext);
        Assert.ThrowsAsync<DbUpdateException>(async () => await Open(10).Create(DbContext));
    }

    [Test]
    public async Task ClosedThenNewOpen_SameLocation_Allowed()
    {
        var first = Open(10);
        await first.Create(DbContext);
        first.ClosedAt = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc);
        first.ClosedBySiteId = 7;
        await first.Update(DbContext);
        await Open(10).Create(DbContext);
        Assert.That(await DbContext.TailBiteOutbreaks.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task RemovedOpenOutbreak_DoesNotBlockNewOne()
    {
        var first = Open(10);
        await first.Create(DbContext);
        await first.Delete(DbContext);
        await Open(10).Create(DbContext);
        Assert.That(await DbContext.TailBiteOutbreaks.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task VersionTable_HasNoOpenKeyColumn()
    {
        var conn = DbContext.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TailBiteOutbreakVersions' AND COLUMN_NAME = 'OpenKey'";
        Assert.That(Convert.ToInt32(await cmd.ExecuteScalarAsync()), Is.EqualTo(0));
    }

    [Test]
    public async Task Link_ToMissingOutbreak_Rejected()
    {
        var reg = new TailBiteRegistration
        {
            PropertyId = 1, SiteId = 7, ClientUuid = Guid.NewGuid(),
            RegisteredAt = new DateTime(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc),
            ReceivedAt = new DateTime(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc),
            EffectiveAt = new DateTime(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc)
        };
        await reg.Create(DbContext);
        var regLoc = new TailBiteRegistrationLocation { RegistrationId = reg.Id, LocationId = 10 };
        await regLoc.Create(DbContext);
        Assert.ThrowsAsync<DbUpdateException>(async () =>
            await new TailBiteOutbreakLink { OutbreakId = 999999, RegistrationLocationId = regLoc.Id }.Create(DbContext));
    }
}
