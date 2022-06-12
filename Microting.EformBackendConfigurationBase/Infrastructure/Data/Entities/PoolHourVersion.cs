using System.ComponentModel.DataAnnotations.Schema;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.EformBackendConfigurationBase.Infrastructure.Enum;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class PoolHourVersion : BaseEntity
{
    public int AreaRuleId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
    public int ItemsPlanningId { get; set; }
    [ForeignKey("PoolHour")]
    public int PoolHourId { get; set; }
    public bool IsActive { get; set; }
}