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

// The tests check the built-in fallback zone directly.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microting.EformBackendConfigurationBase.Tests")]

namespace Microting.EformBackendConfigurationBase.Infrastructure.Helpers;

using System;

/// <summary>
/// The time zone of a property (<c>Property.TimeZone</c>, an IANA id). Null, empty or unknown means
/// <see cref="Default"/>, which is every property today.
/// </summary>
public static class PropertyTimeZones
{
    public const string Default = "Europe/Copenhagen";

    private static readonly Lazy<TimeZoneInfo> DefaultZone = new(() => Find(Default) ?? BuiltInCopenhagen());

    /// <summary>
    /// The zone for <paramref name="timeZoneId"/>, or the <see cref="Default"/> zone for null, empty
    /// or unknown ids. Never throws. If the host has no time zone data, the default falls back to a
    /// built-in CET/CEST rule.
    /// </summary>
    public static TimeZoneInfo Resolve(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId == Default)
        {
            return DefaultZone.Value;
        }

        return Find(timeZoneId) ?? DefaultZone.Value;
    }

    private static TimeZoneInfo Find(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }
    }

    /// <summary>CET (UTC+1) / CEST (UTC+2): summer time from the last Sunday of March 02:00 to the
    /// last Sunday of October 03:00 local time (EU rule since 1996).</summary>
    internal static TimeZoneInfo BuiltInCopenhagen()
    {
        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            DateTime.MinValue.Date,
            DateTime.MaxValue.Date,
            TimeSpan.FromHours(1),
            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday),
            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 10, 5, DayOfWeek.Sunday));
        return TimeZoneInfo.CreateCustomTimeZone(
            Default, TimeSpan.FromHours(1), Default, "CET", "CEST", [rule]);
    }
}
