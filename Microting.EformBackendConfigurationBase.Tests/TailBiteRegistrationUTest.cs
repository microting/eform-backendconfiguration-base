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
public class TailBiteRegistrationUTest : DbTestFixture
{
    private static TailBiteRegistration NewReg(Guid uuid) => new()
    {
        PropertyId = 1, SiteId = 7, ClientUuid = uuid,
        RegisteredAt = new DateTime(2026, 10, 1, 6, 12, 0, DateTimeKind.Utc),
        ReceivedAt = new DateTime(2026, 10, 1, 6, 12, 5, DateTimeKind.Utc),
        EffectiveAt = new DateTime(2026, 10, 1, 6, 12, 0, DateTimeKind.Utc)
    };

    [Test]
    public async Task ClientUuid_IsUnique()
    {
        var uuid = Guid.NewGuid();
        await NewReg(uuid).Create(DbContext);
        Assert.ThrowsAsync<DbUpdateException>(async () => await NewReg(uuid).Create(DbContext));
    }

    [Test]
    public async Task Row_StoresCountsAndLegacySeverity()
    {
        var reg = NewReg(Guid.NewGuid());
        await reg.Create(DbContext);
        await new TailBiteRegistrationLocation { RegistrationId = reg.Id, LocationId = 5, MinorCount = 0, SevereCount = 0, CountUnknown = true, LegacySeverity = TailBiteSeverity.Severe }.Create(DbContext);
        var row = await DbContext.TailBiteRegistrationLocations.SingleAsync();
        Assert.That(row.CountUnknown, Is.True);
        Assert.That(row.LegacySeverity, Is.EqualTo(TailBiteSeverity.Severe));
    }

    [Test]
    public async Task PhotoUuid_IsUnique()
    {
        var p = Guid.NewGuid();
        await new TailBiteRegistrationPhoto { PhotoUuid = p, PropertyId = 1, UploadedBySiteId = 7, RegistrationClientUuid = Guid.NewGuid(), SdkUploadedDataId = 1 }.Create(DbContext);
        Assert.ThrowsAsync<DbUpdateException>(async () =>
            await new TailBiteRegistrationPhoto { PhotoUuid = p, PropertyId = 1, UploadedBySiteId = 7, RegistrationClientUuid = Guid.NewGuid(), SdkUploadedDataId = 2 }.Create(DbContext));
    }

    [Test]
    public async Task SameLocationTwiceInOneRegistration_Rejected()
    {
        var reg = NewReg(Guid.NewGuid());
        await reg.Create(DbContext);
        await new TailBiteRegistrationLocation { RegistrationId = reg.Id, LocationId = 5 }.Create(DbContext);
        Assert.ThrowsAsync<DbUpdateException>(async () =>
            await new TailBiteRegistrationLocation { RegistrationId = reg.Id, LocationId = 5 }.Create(DbContext));
    }
}
