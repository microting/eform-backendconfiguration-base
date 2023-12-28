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

using System;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

using Enum;

public class WorkorderCase : PnBase
{
    public int PropertyWorkerId { get; set; }

    public virtual PropertyWorker PropertyWorker { get; set; }

    public int CaseId { get; set; }

    public CaseStatusesEnum CaseStatusesEnum { get; set; }

    // id case with new task
    public int? ParentWorkorderCaseId { get; set; }

    public virtual WorkorderCase ParentWorkorderCase { get; set; }

    public int? EntityItemIdForArea { get; set; }

    public string SelectedAreaName { get; set; }

    public string CreatedByName { get; set; }

    public string CreatedByText { get; set; }

    public string Description { get; set; }

    public DateTime CaseInitiated { get; set; }

    public string LastUpdatedByName { get; set; }

    public string LastAssignedToName { get; set; }

    public bool LeadingCase { get; set; }

    public string Priority { get; set; }

    public int? CreatedBySdkSiteId { get; set; }

    public int? UpdatedBySdkSiteId { get; set; }

    public int? AssignedToSdkSiteId { get; set; }
}