namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class CalendarOccurrenceExceptionSite : PnBase
{
    public int CalendarOccurrenceExceptionId { get; set; }
    public virtual CalendarOccurrenceException CalendarOccurrenceException { get; set; }
    public int SiteId { get; set; }
}
