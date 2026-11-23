/*
The MIT License (MIT)

Copyright (c) 2007 - 2026 Microting A/S

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
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

// The raw SQL below concatenates only literals, table names and migration ids.
#pragma warning disable EF1002, EF1003

// AddAreaRulePlanningItemPlanningIdIndex: one guarded secondary index, nothing else. Own database:
// the tests roll the schema backwards, which the shared one cannot survive.
[TestFixture]
public class AreaRulePlanningItemPlanningIdIndexMigrationUTest : MigrationTestBase
{
    protected override string DatabaseName => "backend-configuration-pn-arp-item-planning-index-tests";

    protected override string MigrationName => "AddAreaRulePlanningItemPlanningIdIndex";

    private const string Table = "AreaRulePlannings";
    private const string Index = "IX_AreaRulePlannings_ItemPlanningId";

    [Test]
    public void Model_HasNoChangesMissingFromTheSnapshot()
    {
        Assert.That(_dbContext.Database.HasPendingModelChanges(), Is.False);
    }

    [Test]
    public void Migration_OnEmptyDatabase_CreatesTheIndex()
    {
        _dbContext.Database.EnsureDeleted();

        MigrateTo(_migrationUnderTest);

        AssertIndex();
    }

    [Test]
    public void Migration_FromPreviousMigration_AddsOnlyTheIndex()
    {
        Assert.That(IndexColumns(), Is.Empty);
        var before = SchemaFingerprint();

        MigrateTo(_migrationUnderTest);
        var after = SchemaFingerprint();

        AssertIndex();
        Assert.Multiple(() =>
        {
            Assert.That(before.Except(after), Is.Empty, "schema rows that changed or disappeared");
            Assert.That(after.Except(before),
                Is.EqualTo(new[] { Table + "|idx|" + Index + "|1|1|ItemPlanningId" }),
                "the only schema row added");
        });
    }

    [Test]
    public void Migration_RerunAfterTheIndexExists_IsANoOp()
    {
        // The state of a pod that died after the CREATE INDEX but before EF recorded the migration.
        MigrateTo(_migrationUnderTest);
        _dbContext.Database.ExecuteSqlRaw(
            "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '" + _migrationUnderTest + "'");

        Assert.That(() => MigrateTo(_migrationUnderTest), Throws.Nothing);
        AssertIndex();
    }

    [Test]
    public void Down_DropsTheIndex_AndNothingElse()
    {
        var before = SchemaFingerprint();
        MigrateTo(_migrationUnderTest);

        MigrateTo(_previousMigration);

        Assert.That(IndexColumns(), Is.Empty);
        Assert.That(SchemaFingerprint(), Is.EquivalentTo(before));
    }

    private void AssertIndex()
    {
        Assert.That(IndexColumns(), Is.EqualTo(new[] { "ItemPlanningId" }));
        Assert.That(Convert.ToInt32(Scalar(
            "SELECT MAX(NON_UNIQUE) FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
            "AND table_name = '" + Table + "' AND index_name = '" + Index + "'")), Is.EqualTo(1), "non-unique");
    }

    private List<string> IndexColumns() => Strings(
        "SELECT COLUMN_NAME FROM information_schema.STATISTICS WHERE table_schema = DATABASE() " +
        "AND table_name = '" + Table + "' AND index_name = '" + Index + "' ORDER BY SEQ_IN_INDEX");
}
