/*
The MIT License (MIT)

Copyright (c) 2007 - 2023 Microting A/S

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

public class TailBiteRegistrationVersion : PnBase
{
    public int PropertyId { get; set; }
    public int SiteId { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime EffectiveAt { get; set; }
    public Guid ClientUuid { get; set; }
    [StringLength(2000)] public string Comment { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBySiteId { get; set; }
    [StringLength(1000)] public string CancelReason { get; set; }
    public int? LegacyCaseId { get; set; }
    public string LegacyRawText { get; set; }
    public string LegacyRiskAnswers { get; set; }
    public int TailBiteRegistrationId { get; set; }
}
