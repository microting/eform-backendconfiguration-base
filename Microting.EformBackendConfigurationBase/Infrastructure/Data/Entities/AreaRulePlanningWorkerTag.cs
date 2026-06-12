namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class AreaRulePlanningWorkerTag : PnBase
{
    public int AreaRulePlanningId { get; set; }

    public virtual AreaRulePlanning AreaRulePlanning { get; set; }

    // References SDK Tag.Id (different DB — no nav prop, same pattern as PlanningSite.SiteId)
    public int TagId { get; set; }
}
