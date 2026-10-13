namespace Microting.EformBackendConfigurationBase.Infrastructure.Helpers;

using System;
using Enum;

/// <summary>
/// The single definition of "Kræver handling" and of the chemical alert
/// thresholds (flutter-chemistry spec §9). Used by ChemicalAlertJob and
/// ChemicalDigestJob in eform-service-backendconfiguration-plugin and by the
/// plugin's web inventory (W1). The Flutter app ports it to Dart against the
/// vectors in Plan D's Contract (C1) and ChemicalAlertRulesUTest.
///
/// Calendar semantics: "today" is the Europe/Copenhagen date; a deadline is the
/// date part of the stored BMD value (no zone conversion) and is itself still
/// allowed; months are calendar months (DateOnly.AddMonths clamps month ends).
/// </summary>
public static class ChemicalAlertRules
{
    /// <summary>BMD Chemical.Status "Produkt afmeldt".</summary>
    public const int StatusDeregistered = 6;

    /// <summary>BMD Chemical.Status "Produkt udløbet".</summary>
    public const int StatusExpired = 7;

    public const string CopenhagenTimeZoneId = "Europe/Copenhagen";

    // Lazy so a host without tzdata fails where the zone is first needed
    // (inside a job's try/catch + Sentry), not in a static constructor.
    private static readonly Lazy<TimeZoneInfo> CopenhagenZone =
        new(() => TimeZoneInfo.FindSystemTimeZoneById(CopenhagenTimeZoneId));

    public static TimeZoneInfo Copenhagen => CopenhagenZone.Value;

    public static bool IsActionStatus(int? status) => status is StatusDeregistered or StatusExpired;

    public static DateOnly DeadlineDate(DateTime deadline) => DateOnly.FromDateTime(deadline);

    /// <summary>
    /// The most severe threshold reached on <paramref name="today"/>: Passed from
    /// the day after the deadline, OneMonth from deadline − 1 month, SixMonths
    /// from deadline − 6 months; null before that or without a deadline.
    /// </summary>
    public static ChemicalAlertThresholdEnum? ReachedThreshold(DateTime? deadline, DateOnly today)
    {
        if (deadline is not { } value)
        {
            return null;
        }

        var date = DeadlineDate(value);
        if (today > date)
        {
            return ChemicalAlertThresholdEnum.Passed;
        }

        if (today >= date.AddMonths(-1))
        {
            return ChemicalAlertThresholdEnum.OneMonth;
        }

        return today >= date.AddMonths(-6) ? ChemicalAlertThresholdEnum.SixMonths : null;
    }

    public static bool RequiresAction(
        int? status, DateTime? salesDeadline, DateTime? useAndPossessionDeadline, DateOnly today) =>
        IsActionStatus(status)
        || ReachedThreshold(salesDeadline, today) is not null
        || ReachedThreshold(useAndPossessionDeadline, today) is not null;

    public static DateOnly CopenhagenToday(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("utcNow must be a UTC DateTime.", nameof(utcNow));
        }

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, Copenhagen));
    }
}
