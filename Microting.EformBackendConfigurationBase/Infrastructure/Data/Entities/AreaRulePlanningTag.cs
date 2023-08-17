namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class AreaRulePlanningTag : PnBase
{
    public int AreaRulePlanningId { get; set; }

    public virtual AreaRulePlanning AreaRulePlanning { get; set; }

    public int ItemPlanningTagId { get; set; }
}