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
using Enum;

/// <summary>
/// One task reminder's whole life (outbox): the web host plans it, the background service delivers
/// it and, if the task is finished or moved after sending, withdraws it. Change rows only through
/// <see cref="Helpers.TaskNotificationOutbox"/>, which holds the state machine and the concurrency
/// rules.
/// <para>
/// Deliberately NOT a <see cref="PnBase"/>: a log with set-based state changes, no Version or
/// Versions table, and rows older than 30 days are physically deleted (approved exception to the
/// soft-delete rule, for this table and the change-feed table only).
/// </para>
/// All times are UTC. Title and Body are rendered at planning time and contain the task name and
/// time; nothing else personal.
/// </summary>
public class TaskNotification
{
    public long Id { get; set; }

    /// <summary>SDK Site.Id of the worker who gets the reminder.</summary>
    public int SdkSiteId { get; set; }

    public int PropertyId { get; set; }

    /// <summary>The task (hub/gRPC event_id).</summary>
    public int AreaRulePlanningId { get; set; }

    /// <summary>The occurrence's day in the property's time zone, <c>yyyy-MM-dd</c>.</summary>
    [Required]
    [StringLength(10)]
    public string PlanDayKey { get; set; }

    public int? ComplianceId { get; set; }

    /// <summary>One of <see cref="Const.TaskNotificationKinds"/>.</summary>
    [Required]
    [StringLength(16)]
    public string Kind { get; set; }

    public DateTime PlannedStartUtc { get; set; }

    /// <summary>When the reminder is due (the planner's lead time before <see cref="PlannedStartUtc"/>).</summary>
    public DateTime SendAtUtc { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    [StringLength(400)]
    public string Body { get; set; }

    public TaskNotificationState State { get; set; }

    /// <summary>Set while a deliverer holds the row (Sending/Withdrawing).</summary>
    public DateTime? LeaseUntilUtc { get; set; }

    /// <summary>How often the row was claimed, in total. Also the fencing token of a claim: a mark
    /// only applies when the row still carries the attempt number of the claim that makes it.</summary>
    public int Attempts { get; set; }

    /// <summary>The row is not claimed before this time (due time, then back-off).</summary>
    public DateTime NextAttemptAtUtc { get; set; }

    public DateTime? SentAtUtc { get; set; }

    public DateTime? WithdrawnAtUtc { get; set; }

    /// <summary>The last error code (e.g. an FCM error code). Never a token or message text.</summary>
    [StringLength(64)]
    public string LastError { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
