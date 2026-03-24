namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class CalendarConfiguration : PnBase
{
    public int AreaRulePlanningId { get; set; }

    public virtual AreaRulePlanning AreaRulePlanning { get; set; }

    public double StartHour { get; set; }

    public double Duration { get; set; }

    public int? BoardId { get; set; }

    public string Color { get; set; }
}
