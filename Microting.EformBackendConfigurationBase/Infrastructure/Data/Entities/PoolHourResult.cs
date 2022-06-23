using System;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class PoolHourResult : PnBase
{
    [ForeignKey("PoolHour")]
    public int PoolHourId { get; set; }
    public DateTime Date { get; set; }
    public int SdkCaseId { get; set; }
    public int PlanningId { get; set; }
    [ForeignKey("AreaRule")]
    public int AreaRuleId { get; set; }
    public double? PulseRateAtOpening { get; set; }
    public double? ReadPhValue { get; set; }
    public double? ReadFreeChlorine { get; set; }
    public double? ReadTemperature { get; set; }
    public double? NumberOfGuestsAtClosing { get; set; }
    [CanBeNull] public string Clarity { get; set; }
    public double? MeasuredFreeChlorine { get; set; }
    public double? MeasuredTotalChlorine { get; set; }
    public double? MeasuredBoundChlorine { get; set; }
    public double? MeasuredPh { get; set; }
    [CanBeNull] public string AcknowledgmentOfPulseRateAtOpening { get; set; }
    public double? MeasuredTempDuringTheDay { get; set; }
    [CanBeNull] public string Comment { get; set; }
    public int? DoneByUserId { get; set; }
    public int FolderId { get; set; }
    [CanBeNull] public string DoneByUserName { get; set; }

}