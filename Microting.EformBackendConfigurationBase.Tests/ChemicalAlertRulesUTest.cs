namespace Microting.EformBackendConfigurationBase.Tests;

using System;
using System.Globalization;
using Infrastructure.Enum;
using Infrastructure.Helpers;
using NUnit.Framework;

/// <summary>
/// Pins the one definition of "Kræver handling" (flutter-chemistry spec §9).
/// The same vectors are the Dart port's contract (Plan D, section C1): change
/// them only together with Plan B.
/// </summary>
[TestFixture]
public class ChemicalAlertRulesUTest
{
    private static DateOnly Day(string iso) => DateOnly.ParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static DateTime UtcMidnight(string iso) => Day(iso).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    [TestCase("2026-09-29", null)]
    [TestCase("2026-09-30", ChemicalAlertThresholdEnum.SixMonths)]
    [TestCase("2027-02-27", ChemicalAlertThresholdEnum.SixMonths)]
    [TestCase("2027-02-28", ChemicalAlertThresholdEnum.OneMonth)]
    [TestCase("2027-03-31", ChemicalAlertThresholdEnum.OneMonth)]
    [TestCase("2027-04-01", ChemicalAlertThresholdEnum.Passed)]
    public void ReachedThreshold_MonthEndDeadline(string today, ChemicalAlertThresholdEnum? expected)
    {
        Assert.That(ChemicalAlertRules.ReachedThreshold(UtcMidnight("2027-03-31"), Day(today)), Is.EqualTo(expected));
    }

    [TestCase("2027-08-28", null)]
    [TestCase("2027-08-29", ChemicalAlertThresholdEnum.SixMonths)]
    [TestCase("2028-01-28", ChemicalAlertThresholdEnum.SixMonths)]
    [TestCase("2028-01-29", ChemicalAlertThresholdEnum.OneMonth)]
    [TestCase("2028-02-29", ChemicalAlertThresholdEnum.OneMonth)]
    [TestCase("2028-03-01", ChemicalAlertThresholdEnum.Passed)]
    public void ReachedThreshold_LeapDayDeadline(string today, ChemicalAlertThresholdEnum? expected)
    {
        Assert.That(ChemicalAlertRules.ReachedThreshold(UtcMidnight("2028-02-29"), Day(today)), Is.EqualTo(expected));
    }

    [Test]
    public void ReachedThreshold_UsesTheStoredDatePart_WithoutZoneConversion()
    {
        var lateEvening = new DateTime(2027, 3, 31, 22, 0, 0, DateTimeKind.Utc);

        Assert.That(ChemicalAlertRules.ReachedThreshold(lateEvening, Day("2027-03-31")),
            Is.EqualTo(ChemicalAlertThresholdEnum.OneMonth));
        Assert.That(ChemicalAlertRules.ReachedThreshold(lateEvening, Day("2027-04-01")),
            Is.EqualTo(ChemicalAlertThresholdEnum.Passed));
    }

    [Test]
    public void ReachedThreshold_NoDeadline_IsNull()
    {
        Assert.That(ChemicalAlertRules.ReachedThreshold(null, Day("2030-01-01")), Is.Null);
    }

    [TestCase(null, false)]
    [TestCase(5, false)]
    [TestCase(6, true)]
    [TestCase(7, true)]
    [TestCase(8, false)]
    public void IsActionStatus_OnlyDeregisteredAndExpired(int? status, bool expected)
    {
        Assert.That(ChemicalAlertRules.IsActionStatus(status), Is.EqualTo(expected));
    }

    [Test]
    public void RequiresAction_Vectors()
    {
        var today = Day("2026-10-01");
        var farAway = UtcMidnight("2030-01-01");

        Assert.That(ChemicalAlertRules.RequiresAction(5, null, null, today), Is.False);
        Assert.That(ChemicalAlertRules.RequiresAction(null, null, null, today), Is.False);
        Assert.That(ChemicalAlertRules.RequiresAction(6, null, null, today), Is.True);
        Assert.That(ChemicalAlertRules.RequiresAction(7, farAway, farAway, today), Is.True);
        Assert.That(ChemicalAlertRules.RequiresAction(5, farAway, farAway, today), Is.False);
        Assert.That(ChemicalAlertRules.RequiresAction(5, UtcMidnight("2027-03-31"), null, today), Is.True);
        Assert.That(ChemicalAlertRules.RequiresAction(5, null, UtcMidnight("2026-09-30"), today), Is.True);
    }

    // 2026-10-25 03:00 CEST -> 02:00 CET (DST ends); 2027-03-28 02:00 CET -> 03:00 CEST (DST starts).
    [TestCase("2026-10-24T21:59:00Z", "2026-10-24")] // 23:59 CEST
    [TestCase("2026-10-24T22:00:00Z", "2026-10-25")] // 00:00 CEST
    [TestCase("2026-10-25T22:59:00Z", "2026-10-25")] // 23:59 CET
    [TestCase("2026-10-25T23:00:00Z", "2026-10-26")] // 00:00 CET
    [TestCase("2027-03-27T22:59:00Z", "2027-03-27")] // 23:59 CET
    [TestCase("2027-03-27T23:00:00Z", "2027-03-28")] // 00:00 CET
    [TestCase("2027-03-28T21:59:00Z", "2027-03-28")] // 23:59 CEST
    [TestCase("2027-03-28T22:00:00Z", "2027-03-29")] // 00:00 CEST
    public void CopenhagenToday_FollowsDanishCalendarAcrossDst(string utcIso, string expected)
    {
        var utcNow = DateTime.Parse(utcIso, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

        Assert.That(utcNow.Kind, Is.EqualTo(DateTimeKind.Utc), "arrange: the parsed instant must be UTC");
        Assert.That(ChemicalAlertRules.CopenhagenToday(utcNow), Is.EqualTo(Day(expected)));
    }

    [Test]
    public void CopenhagenToday_RejectsNonUtcInput()
    {
        Assert.That(() => ChemicalAlertRules.CopenhagenToday(new DateTime(2026, 10, 1, 5, 0, 0, DateTimeKind.Local)),
            Throws.ArgumentException);
        Assert.That(() => ChemicalAlertRules.CopenhagenToday(new DateTime(2026, 10, 1, 5, 0, 0, DateTimeKind.Unspecified)),
            Throws.ArgumentException);
    }

    [Test]
    public void Copenhagen_IsTheIanaZone()
    {
        Assert.That(ChemicalAlertRules.Copenhagen.Id, Is.EqualTo(ChemicalAlertRules.CopenhagenTimeZoneId));
    }
}
