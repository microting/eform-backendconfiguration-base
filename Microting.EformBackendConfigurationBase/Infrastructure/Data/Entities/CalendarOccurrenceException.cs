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
    public virtual List<CalendarOccurrenceExceptionSite> ExceptionSites { get; set; } = [];
}
