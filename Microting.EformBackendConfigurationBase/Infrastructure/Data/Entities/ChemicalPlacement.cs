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

/// <summary>
/// A register product kept at a ChemicalLocation. Open while RemovedAt is null.
/// ChemicalId/ProductId reference the customer's chemical-base copy (no FK across databases).
/// </summary>
public class ChemicalPlacement : PnBase
{
    public int LocationId { get; set; }

    [ForeignKey("LocationId")]
    public virtual ChemicalLocation Location { get; set; }

    public int ChemicalId { get; set; }

    public int? ProductId { get; set; }

    public string PlacementNote { get; set; }

    /// <summary>eForm user id of the person who registered it.</summary>
    public int RegisteredByUserId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public int? RemovedByUserId { get; set; }

    public DateTime? RemovedAt { get; set; }

    public ChemicalRemovalReasonEnum? RemovalReason { get; set; }

    public string RemovalNote { get; set; }

    public int? MovedFromPlacementId { get; set; }

    /// <summary>BMD Chemical.Status seen at registration; the alert job updates it on change.</summary>
    public int? ObservedStatus { get; set; }
}
