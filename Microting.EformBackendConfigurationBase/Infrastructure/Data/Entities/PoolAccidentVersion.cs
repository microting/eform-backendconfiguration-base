using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microting.eFormApi.BasePn.Infrastructure.Database.Base;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class PoolAccidentVersion : BaseEntity
{
    [ForeignKey("PoolAccident")]
    public int PoolAccidentId { get; set; }
    public int AreaRuleId { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly Time { get; set; }
    public bool SolidFaeces { get; set; }
    public bool DiarrheaLoose { get; set; }
    public bool Vomit { get; set; }
    public int ContactedPersonId { get; set; }
    public int OwnPersonId { get; set; }
    public string Comment { get; set; }
}