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
using Infrastructure.Helpers;
using NUnit.Framework;

[TestFixture]
public class PropertyTimeZonesUTest
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("Mars/Olympus_Mons")]
    [TestCase("Europe/Copenhagen")]
    public void Resolve_FallsBackToCopenhagen_ForNullEmptyOrUnknownIds(string id)
    {
        var zone = PropertyTimeZones.Resolve(id);

        Assert.That(PropertyTimeZones.Default, Is.EqualTo("Europe/Copenhagen"));
        Assert.That(zone.Id, Is.EqualTo("Europe/Copenhagen"));
        AssertCopenhagenRules(zone);
    }

    [Test]
    public void Resolve_ReturnsAValidIanaZone()
    {
        var zone = PropertyTimeZones.Resolve("America/New_York");

        Assert.That(zone.Id, Is.EqualTo("America/New_York"));
        Assert.That(TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc), zone).Hour, Is.EqualTo(8));
    }

    // Used only when the host has no time zone data; must agree with the system zone.
    [Test]
    public void BuiltInCopenhagen_FollowsTheEuRules()
    {
        AssertCopenhagenRules(PropertyTimeZones.BuiltInCopenhagen());
    }

    // 2026: summer time from 29 March 01:00 UTC (02:00 CET -> 03:00 CEST) to 25 October 01:00 UTC
    // (03:00 CEST -> 02:00 CET).
    private static void AssertCopenhagenRules(TimeZoneInfo zone)
    {
        static DateTime Local(TimeZoneInfo z, int month, int day, int hour, int minute) =>
            TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2026, month, day, hour, minute, 0, DateTimeKind.Utc), z);

        Assert.Multiple(() =>
        {
            Assert.That(Local(zone, 1, 15, 12, 0), Is.EqualTo(new DateTime(2026, 1, 15, 13, 0, 0)), "winter");
            Assert.That(Local(zone, 7, 15, 12, 0), Is.EqualTo(new DateTime(2026, 7, 15, 14, 0, 0)), "summer");
            Assert.That(Local(zone, 3, 29, 0, 59), Is.EqualTo(new DateTime(2026, 3, 29, 1, 59, 0)), "just before spring forward");
            Assert.That(Local(zone, 3, 29, 1, 0), Is.EqualTo(new DateTime(2026, 3, 29, 3, 0, 0)), "spring forward");
            Assert.That(Local(zone, 10, 25, 0, 59), Is.EqualTo(new DateTime(2026, 10, 25, 2, 59, 0)), "just before fall back");
            Assert.That(Local(zone, 10, 25, 1, 0), Is.EqualTo(new DateTime(2026, 10, 25, 2, 0, 0)), "fall back");
            Assert.That(zone.IsInvalidTime(new DateTime(2026, 3, 29, 2, 30, 0)), Is.True, "02:30 does not exist on 29 March");
            Assert.That(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2026, 11, 23, 7, 0, 0), zone),
                Is.EqualTo(new DateTime(2026, 11, 23, 6, 0, 0)), "07:00 local in November is 06:00 UTC");
        });
    }
}
