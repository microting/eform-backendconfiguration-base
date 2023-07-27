using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class PoolHistorySite : PnBase
{
    [ForeignKey("AreaRuleId")]
    public int AreaRuleId { get; set; }
    public virtual AreaRule AreaRule { get; set; }
    public DateTime Date { get; set; }
    public int SiteId { get; set; }
    public int SdkCaseId { get; set; }
}