/*
The MIT License (MIT)

Copyright (c) 2007 - 2026 Microting A/S

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

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// One row of the cross-process task change feed (live updates). A process that cannot reach the
/// web host's hub (the background service) appends a row; the web host polls the table by
/// <see cref="Id"/> and forwards each row as an ids-only hint. Write and read it through
/// <see cref="Helpers.TaskChangeFeed"/>.
/// <para>
/// Deliberately NOT a <see cref="PnBase"/> (nor a BaseEntity): the rows are append-only, never
/// updated, and physically deleted after 24 hours (approved exception to the soft-delete rule, for
/// this log table only). WorkflowState, Version, UpdatedAt, the user columns and a Versions table
/// would only add write cost and keep the table growing.
/// </para>
/// Ids and keys only: no names, no free text, no personal data.
/// </summary>
public class TaskChangeFeedEntry
{
    /// <summary>The reader's cursor. bigint auto-increment: allocated in increasing order (see
    /// <see cref="Helpers.TaskChangeFeed"/> for the commit-order caveat).</summary>
    public long Id { get; set; }

    /// <summary>UTC time of the append. Used only by the 24-hour clean-up.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Hub <c>property_id</c>. Null for a change that is not tied to one property
    /// (e.g. <see cref="Const.TaskChangeKinds.Assignments"/>).</summary>
    public int? PropertyId { get; set; }

    /// <summary>SDK Site.Id of the worker concerned (<see cref="Const.TaskChangeKinds.Assignments"/>).</summary>
    public int? SdkSiteId { get; set; }

    /// <summary>Hub <c>event_id</c>, i.e. gRPC <c>EventKey.event_id</c> (the AreaRulePlanning id).</summary>
    public int? AreaRulePlanningId { get; set; }

    /// <summary>Hub <c>plan_day_key</c>, <c>yyyy-MM-dd</c>.</summary>
    [StringLength(10)]
    public string PlanDayKey { get; set; }

    /// <summary>Hub <c>compliance_id</c>.</summary>
    public int? ComplianceId { get; set; }

    /// <summary>One of <see cref="Const.TaskChangeKinds"/> (the hub contract's wire names).</summary>
    [Required]
    [StringLength(16)]
    public string Kind { get; set; }

    /// <summary>Name of the writer (job or handler), for diagnostics only.</summary>
    [StringLength(64)]
    public string Source { get; set; }
}
