using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class PoolAccident : PnBase
{
    [ForeignKey("AreaRule")]
    public int AreaRuleId { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly Time { get; set; }
    public bool SolidFaeces { get; set; }
    public bool DiarrheaLoose { get; set; }
    public bool Vomit { get; set; }
    public int ContactedPersonId { get; set; }
    public int OwnPersonId { get; set; }
    public string Comment { get; set; }
    public int FolderId { get; set; }
    public int SdkCaseId { get; set; }
    public int PlanningId { get; set; }
}