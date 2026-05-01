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
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class AreaRulePlanningUTest : DbTestFixture
{
    private int _areaRuleId;

    protected override void DoSetup()
    {
        // The shared DbTestFixture.ClearDb only truncates a fixed list that
        // doesn't include AreaRulePlannings or its parent rows, so wipe the
        // tables this test exercises before each run.
        DbContext.Database.ExecuteSqlRaw(
            "SET FOREIGN_KEY_CHECKS = 0; " +
            "TRUNCATE `AreaRulesPlanningVersions`; " +
            "TRUNCATE `AreaRulePlannings`; " +
            "TRUNCATE `AreaRuleVersions`; " +
            "TRUNCATE `AreaRules`; " +
            "TRUNCATE `AreaInitialFieldVersions`; " +
            "TRUNCATE `AreaInitialFields`; " +
            "TRUNCATE `AreaTranslationVersions`; " +
            "TRUNCATE `AreaTranslations`; " +
            "TRUNCATE `AreaVersions`; " +
            "TRUNCATE `Areas`; " +
            "TRUNCATE `PropertieVersions`; " +
            "TRUNCATE `Properties`;");

        // Build a minimal Property → Area → AreaRule chain so AreaRulePlanning
        // can satisfy its non-nullable FK to AreaRule.
        var property = new Property
        {
            Address = Guid.NewGuid().ToString(),
            CHR = Guid.NewGuid().ToString(),
            Name = Guid.NewGuid().ToString(),
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        property.Create(DbContext).GetAwaiter().GetResult();

        var area = new Area
        {
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        area.Create(DbContext).GetAwaiter().GetResult();

        var areaRule = new AreaRule
        {
            AreaId = area.Id,
            PropertyId = property.Id,
            CreatedByUserId = 1,
            UpdatedByUserId = 1,
        };
        areaRule.Create(DbContext).GetAwaiter().GetResult();

        _areaRuleId = areaRule.Id;
    }

    private AreaRulePlanning NewPlanning(string? csv) => new()
    {
        AreaRuleId = _areaRuleId,
        CreatedByUserId = 1,
        UpdatedByUserId = 1,
        RepeatType = 2,
        RepeatEvery = 1,
        RepeatWeekdaysCsv = csv,
    };

    [Test]
    public async Task AreaRulePlanning_RepeatWeekdaysCsv_RoundTripsCsvValue()
    {
        // Arrange
        var planning = NewPlanning("1,3,5");
        planning.RepeatEvery = 2;
        planning.RepeatEndMode = 1;
        planning.RepeatOccurrences = 10;

        // Act
        await planning.Create(DbContext);

        var planningList = DbContext.AreaRulePlannings.AsNoTracking().ToList();
        var planningVersionList = DbContext.AreaRulesPlanningVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(planningList.Count, Is.EqualTo(1));
        Assert.That(planningList[0].RepeatWeekdaysCsv, Is.EqualTo("1,3,5"));
        Assert.That(planningVersionList.Count, Is.EqualTo(1));
        Assert.That(planningVersionList[0].RepeatWeekdaysCsv, Is.EqualTo("1,3,5"));
    }

    [Test]
    public async Task AreaRulePlanning_RepeatWeekdaysCsv_RoundTripsNull()
    {
        // Arrange
        var planning = NewPlanning(null);

        // Act
        await planning.Create(DbContext);

        var planningList = DbContext.AreaRulePlannings.AsNoTracking().ToList();
        var planningVersionList = DbContext.AreaRulesPlanningVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(planningList.Count, Is.EqualTo(1));
        Assert.That(planningList[0].RepeatWeekdaysCsv, Is.Null);
        Assert.That(planningVersionList.Count, Is.EqualTo(1));
        Assert.That(planningVersionList[0].RepeatWeekdaysCsv, Is.Null);
    }

    [Test]
    public async Task AreaRulePlanning_RepeatWeekdaysCsv_PersistsAllSevenWeekdaysAtMaxLength()
    {
        // Arrange — "0,1,2,3,4,5,6" is exactly 13 chars, the column's [StringLength(13)] cap.
        // This is the longest valid CSV the domain produces and proves the column width
        // is sized correctly (a too-tight cap would truncate or fail on insert).
        const string allWeekdays = "0,1,2,3,4,5,6";
        Assert.That(allWeekdays.Length, Is.EqualTo(13));
        var planning = NewPlanning(allWeekdays);

        // Act
        await planning.Create(DbContext);

        var planningList = DbContext.AreaRulePlannings.AsNoTracking().ToList();
        var planningVersionList = DbContext.AreaRulesPlanningVersions.AsNoTracking().ToList();

        // Assert
        Assert.That(planningList.Count, Is.EqualTo(1));
        Assert.That(planningList[0].RepeatWeekdaysCsv, Is.EqualTo(allWeekdays));
        Assert.That(planningVersionList.Count, Is.EqualTo(1));
        Assert.That(planningVersionList[0].RepeatWeekdaysCsv, Is.EqualTo(allWeekdays));
    }

    [Test]
    public async Task AreaRulePlanning_RepeatWeekdaysCsv_PreservesValueOnUpdate()
    {
        // Arrange
        var planning = NewPlanning("1,3,5");
        await planning.Create(DbContext);

        // Act — update to a different multi-day pattern
        planning.RepeatWeekdaysCsv = "0,1,2,3,4,5,6";
        await planning.Update(DbContext);

        var planningList = DbContext.AreaRulePlannings.AsNoTracking().ToList();
        var planningVersionList = DbContext.AreaRulesPlanningVersions
            .AsNoTracking()
            .OrderBy(v => v.Version)
            .ToList();

        // Assert
        Assert.That(planningList.Count, Is.EqualTo(1));
        Assert.That(planningList[0].RepeatWeekdaysCsv, Is.EqualTo("0,1,2,3,4,5,6"));
        Assert.That(planningVersionList.Count, Is.EqualTo(2));
        Assert.That(planningVersionList[0].RepeatWeekdaysCsv, Is.EqualTo("1,3,5"));
        Assert.That(planningVersionList[1].RepeatWeekdaysCsv, Is.EqualTo("0,1,2,3,4,5,6"));
    }

    [Test]
    public async Task AreaRulePlanning_RepeatWeekdaysCsv_CanBeClearedToNullOnUpdate()
    {
        // Arrange — start with a multi-day rule
        var planning = NewPlanning("1,3,5");
        await planning.Create(DbContext);

        // Act — clear the CSV (e.g. user switched from weeklyMulti back to weeklyOne)
        planning.RepeatWeekdaysCsv = null;
        await planning.Update(DbContext);

        var planningList = DbContext.AreaRulePlannings.AsNoTracking().ToList();

        // Assert
        Assert.That(planningList.Count, Is.EqualTo(1));
        Assert.That(planningList[0].RepeatWeekdaysCsv, Is.Null);
    }
}
