/*
The MIT License (MIT)

Copyright (c) 2007 - 2022 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using Enum;

    public class AreaRulePlanning: PnBase
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int DayOfWeek { get; set; }

        public int DayOfMonth { get; set; }

        public int? RepeatEvery { get; set; }

        public int? RepeatType { get; set; }

        public bool Status { get; set; }

        public bool SendNotifications { get; set; }

        public AreaRuleT2AlarmsEnum Alarm { get; set; }

        public AreaRuleT2TypesEnum Type { get; set; }

        public int AreaRuleId { get; set; }

        public virtual AreaRule AreaRule { get; set; }

        public int ItemPlanningId { get; set; }

        /// <summary>Gets or sets the item planning tag identifier.
        /// Need for reports</summary>
        /// <value>The item planning tag identifier.</value>
        public int? ItemPlanningTagId { get; set; }

        public int FolderId { get; set; }

        public bool HoursAndEnergyEnabled { get; set; }

        public virtual List<PlanningSite> PlanningSites { get; set; }
            = new();

        public int PropertyId { get; set; }

        public int AreaId { get; set; }

        public bool ComplianceEnabled { get; set; }

        public bool UseStartDateAsStartOfPeriod { get; set; }
    }
}