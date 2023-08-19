﻿/*
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

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

using Enum;

public class AreaRuleVersion : PnBase
{
    public int AreaRuleId { get; set; }

    public int AreaId { get; set; }

    public int? EformId { get; set; }

    public string EformName { get; set; }

    public int FolderId { get; set; }

    public string FolderName { get; set; }

    public AreaRuleT2AlarmsEnum? Alarm { get; set; }

    public AreaRuleT2TypesEnum? Type { get; set; }

    public bool? ChecklistStable { get; set; }

    public bool? TailBite { get; set; }

    public int DayOfWeek { get; set; }

    public int GroupItemId { get; set; }

    public int PropertyId { get; set; }

    public bool IsDefault { get; set; }

    public int RepeatEvery { get; set; }

    public int? RepeatType { get; set; }

    public bool? ComplianceEnabled { get; set; }

    public bool ComplianceModifiable { get; set; }

    public bool? Notifications { get; set; }

    public bool NotificationsModifiable { get; set; }

    public int SecondaryeFormId { get; set; }

    public string SecondaryeFormName { get; set; }

    public bool CreatedInGuide { get; set; }
}