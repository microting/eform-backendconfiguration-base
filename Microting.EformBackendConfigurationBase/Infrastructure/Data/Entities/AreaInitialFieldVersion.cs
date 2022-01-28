using System;
using Microting.EformBackendConfigurationBase.Infrastructure.Enum;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities
{
    public class AreaInitialFieldVersion: PnBase
    {
        public string EformName { get; set; }

        public bool Notifications { get; set; }

        public int? RepeatEvery { get; set; }

        public int? RepeatType { get; set; }

        public int? DayOfWeek { get; set; }

        public AreaRuleT2TypesEnum? Type { get; set; }

        public AreaRuleT2AlarmsEnum? Alarm { get; set; }

        public DateTime? EndDate { get; set; }

        public int AreaId { get; set; }

        public virtual Area Area { get; set; }

        public int AreaInitialFieldId { get; set; }

        public bool ComplianceEnabled { get; set; }
    }
}