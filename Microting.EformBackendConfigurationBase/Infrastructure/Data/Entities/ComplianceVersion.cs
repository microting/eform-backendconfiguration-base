using System;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class ComplianceVersion : PnBase
{
    public string ItemName { get; set; }
    public string AreaId { get; set; }
    public string AreaName { get; set; }
    public int PlanningId { get; set; }
    public int PropertyId { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime StartDate { get; set; }
    public int ComplianceId { get; set; }
}