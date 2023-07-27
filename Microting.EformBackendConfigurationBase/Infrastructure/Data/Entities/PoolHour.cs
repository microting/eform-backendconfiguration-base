using System.ComponentModel.DataAnnotations.Schema;
using Microting.EformBackendConfigurationBase.Infrastructure.Enum;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class PoolHour : PnBase
{
    [ForeignKey("AreaRule")]
    public int AreaRuleId { get; set; }
    public virtual AreaRule AreaRule { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
    public int? ItemsPlanningId { get; set; }
    public bool IsActive { get; set; }
}