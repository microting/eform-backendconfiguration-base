using System;
using System.Collections.Generic;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class CalendarOccurrenceException : PnBase
{
    public int AreaRulePlanningId { get; set; }
    public virtual AreaRulePlanning AreaRulePlanning { get; set; }
    public DateTime OriginalDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? NewDate { get; set; }
    public double? StartHour { get; set; }
    public double? Duration { get; set; }

    // Per-occurrence field overrides for scope="this" edits (issue #885).
    // Null means "inherit the series value" when rendering the occurrence;
    // a non-null value overrides just this occurrence. eForm, tags, status,
    // compliance and repeat stay series-level only and are intentionally not
    // overridable per occurrence.
    public string Title { get; set; }
    public string DescriptionHtml { get; set; }
    public int? BoardId { get; set; }
    public string Color { get; set; }

    public virtual List<CalendarOccurrenceExceptionSite> ExceptionSites { get; set; } = [];
}
