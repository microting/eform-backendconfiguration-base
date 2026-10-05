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
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class TailBiteLocationUTest : DbTestFixture
{
    [Test]
    public async Task Location_CreateAndUpdate_WritesVersions()
    {
        var root = new TailBiteLocation { PropertyId = 1, ParentId = null, Name = "Ejendom", QrCode = "code-root-000000000000" };
        await root.Create(DbContext);
        var stable = new TailBiteLocation { PropertyId = 1, ParentId = root.Id, Name = "Stald 1", QrCode = "code-stable-0000000000" };
        await stable.Create(DbContext);
        stable.Name = "Stald 1A";
        await stable.Update(DbContext);

        var versions = await DbContext.TailBiteLocationVersions
            .Where(v => v.TailBiteLocationId == stable.Id)
            .OrderBy(v => v.Id)
            .ToListAsync();
        Assert.That(versions, Has.Count.EqualTo(2));
        Assert.That(versions.Last().Name, Is.EqualTo("Stald 1A"));
    }

    [Test]
    public async Task QrCode_IsUnique()
    {
        var code = "dup-code-0000000000000";
        await new TailBiteLocation { PropertyId = 1, Name = "A", QrCode = code }.Create(DbContext);
        Assert.ThrowsAsync<DbUpdateException>(async () =>
            await new TailBiteLocation { PropertyId = 1, Name = "B", QrCode = code }.Create(DbContext));
    }
}
