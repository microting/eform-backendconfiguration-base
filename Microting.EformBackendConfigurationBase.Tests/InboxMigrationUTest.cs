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
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class InboxMigrationUTest : DbTestFixture
{
    [Test]
    public async Task Migration_CreatesUniqueHubDocumentIdIndex()
    {
        var conn = DbContext.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT NON_UNIQUE FROM information_schema.STATISTICS " +
            "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'InboxDocuments' " +
            "AND COLUMN_NAME = 'HubDocumentId'";
        var nonUnique = await cmd.ExecuteScalarAsync();

        Assert.That(nonUnique, Is.Not.Null, "index on InboxDocuments.HubDocumentId is missing");
        Assert.That(System.Convert.ToInt32(nonUnique), Is.EqualTo(0), "index must be unique");
    }

    [Test]
    public void Migration_IsApplied()
    {
        Assert.That(DbContext.Database.GetMigrations().Any(m => m.EndsWith("_AddInboundMailInbox")), Is.True);
    }
}
