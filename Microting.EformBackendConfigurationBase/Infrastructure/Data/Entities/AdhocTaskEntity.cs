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

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Named "AdhocTaskEntity" (not "AdhocTask") to avoid a C# name collision with
// the generated proto message "AdhocTask" in the plugin.
public class AdhocTaskEntity : PnBase
{
    [StringLength(250)]
    public string Title { get; set; }

    public string Description { get; set; }

    public bool Urgent { get; set; }

    public int PropertyId { get; set; }

    [ForeignKey("PropertyId")]
    public virtual Property Property { get; set; }

    public int? AreaId { get; set; }

    [ForeignKey("AreaId")]
    public virtual AdhocArea Area { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public DateTime? Deadline { get; set; }

    public bool VisibleReminder { get; set; }

    public bool DeadlineReminder { get; set; }

    // 0 = none, 1 = weekdays
    public int DeadlineReminderRepeat { get; set; }

    // Minutes from midnight
    public int VisibleReminderTimeMinutes { get; set; } = 480;

    // Minutes from midnight
    public int DeadlineReminderTimeMinutes { get; set; } = 480;

    // 0 = assignedOnly, 1 = everyone
    public int ExecutionRule { get; set; }

    public int CreatedByWorkerId { get; set; }

    public bool Completed { get; set; }

    public int? CompletedByWorkerId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool Archived { get; set; }

    public DateTime? ArchivedAt { get; set; }

    // Idempotency markers for reminder push delivery
    public DateTime? LastVisibleReminderSentAt { get; set; }

    public DateTime? LastDeadlineReminderSentAt { get; set; }
}
