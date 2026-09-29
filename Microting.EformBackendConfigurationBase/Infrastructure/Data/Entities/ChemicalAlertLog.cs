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
using System.ComponentModel.DataAnnotations.Schema;
using Enum;

/// <summary>One sent alert (push or digest e-mail); the dedupe source for the alert jobs.</summary>
public class ChemicalAlertLog : PnBase
{
    public int PlacementId { get; set; }

    [ForeignKey("PlacementId")]
    public virtual ChemicalPlacement Placement { get; set; }

    public ChemicalAlertThresholdEnum Threshold { get; set; }

    public ChemicalDeadlineKindEnum? DeadlineKind { get; set; }

    /// <summary>BMD status that triggered a StatusChanged alert.</summary>
    public int? Status { get; set; }

    public ChemicalAlertChannelEnum Channel { get; set; }

    public DateTime SentAt { get; set; }
}
