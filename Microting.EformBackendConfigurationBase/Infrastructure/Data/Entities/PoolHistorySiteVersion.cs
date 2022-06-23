using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microting.eFormApi.BasePn.Infrastructure.Database.Base;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class PoolHistorySiteVersion : BaseEntity
{
    public int AreaRuleId { get; set; }
    public DateTime Date { get; set; }
    public int SiteId { get; set; }
    public int SdkCaseId { get; set; }
    [ForeignKey("PoolHistorySite")]
    public int PoolHistorySiteId { get; set; }
    
}