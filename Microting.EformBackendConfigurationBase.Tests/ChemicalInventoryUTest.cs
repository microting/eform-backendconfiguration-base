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

namespace Microting.EformBackendConfigurationBase.Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using eForm.Infrastructure.Constants;
using Infrastructure.Data.Entities;
using Infrastructure.Enum;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class ChemicalInventoryUTest : DbTestFixture
{
    private static string GetRandomStr() => Guid.NewGuid().ToString();

    private async Task<Property> CreateProperty()
    {
        var property = new Property
        {
            Address = GetRandomStr(), CHR = GetRandomStr(), Name = GetRandomStr(),
            CreatedByUserId = 1, UpdatedByUserId = 1,
        };
        await property.Create(DbContext);
        return property;
    }

    private async Task<ChemicalLocation> CreateLocation(Property property)
    {
        var location = new ChemicalLocation
        {
            PropertyId = property.Id, Name = "Kemirum", Description = "Bag laden", SortOrder = 1,
            CreatedByUserId = 1, UpdatedByUserId = 1,
        };
        await location.Create(DbContext);
        return location;
    }

    private async Task<ChemicalPlacement> CreatePlacement(ChemicalLocation location)
    {
        var placement = new ChemicalPlacement
        {
            LocationId = location.Id, ChemicalId = 4711, ProductId = null, PlacementNote = "Hylde 2",
            RegisteredByUserId = 3, RegisteredAt = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
            ObservedStatus = 5, CreatedByUserId = 3, UpdatedByUserId = 3,
        };
        await placement.Create(DbContext);
        return placement;
    }

    [Test]
    public async Task ChemicalLocation_CreateAndUpdate_WritesOneVersionPerChange()
    {
        var location = await CreateLocation(await CreateProperty());

        location.Name = "Kemiskab";
        location.PhotoFileName = "chemical-location-1-abc.jpg";
        await location.Update(DbContext);

        var stored = await DbContext.ChemicalLocations.AsNoTracking().SingleAsync(x => x.Id == location.Id);
        var versions = await DbContext.ChemicalLocationVersions.AsNoTracking()
            .Where(x => x.ChemicalLocationId == location.Id).OrderBy(x => x.Version).ToListAsync();

        Assert.That(stored.Name, Is.EqualTo("Kemiskab"));
        Assert.That(stored.Version, Is.EqualTo(2));
        Assert.That(versions.Select(x => x.Name), Is.EqualTo(new[] { "Kemirum", "Kemiskab" }));
        Assert.That(versions[1].PhotoFileName, Is.EqualTo("chemical-location-1-abc.jpg"));
        Assert.That(versions[1].SortOrder, Is.EqualTo(1));
    }

    [Test]
    public async Task ChemicalPlacement_Close_PersistsNullableFieldsAndReasonEnum()
    {
        var placement = await CreatePlacement(await CreateLocation(await CreateProperty()));

        placement.RemovedAt = new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc);
        placement.RemovedByUserId = 4;
        placement.RemovalReason = ChemicalRemovalReasonEnum.Disposed;
        placement.RemovalNote = "Afleveret på genbrugsstationen";
        await placement.Update(DbContext);

        var stored = await DbContext.ChemicalPlacements.AsNoTracking().SingleAsync(x => x.Id == placement.Id);
        var version = await DbContext.ChemicalPlacementVersions.AsNoTracking()
            .SingleAsync(x => x.ChemicalPlacementId == placement.Id && x.Version == 2);

        Assert.That(stored.ProductId, Is.Null);
        Assert.That(stored.RemovalReason, Is.EqualTo(ChemicalRemovalReasonEnum.Disposed));
        Assert.That(stored.ObservedStatus, Is.EqualTo(5));
        Assert.That(version.RemovalReason, Is.EqualTo(ChemicalRemovalReasonEnum.Disposed));
        Assert.That(version.RemovedByUserId, Is.EqualTo(4));
    }

    [Test]
    public async Task ChemicalStockEntry_Amounts_KeepThreeDecimalsAndSign()
    {
        var placement = await CreatePlacement(await CreateLocation(await CreateProperty()));
        var entry = new ChemicalStockEntry
        {
            PlacementId = placement.Id, Kind = ChemicalStockEntryKindEnum.Consumed, Unit = ChemicalStockUnitEnum.L,
            Amount = -1.125m, ContainerSize = 0.375m, ContainerCount = 3, BatchLot = "LOT-7",
            ByUserId = 3, At = new DateTime(2026, 9, 2, 8, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 3, UpdatedByUserId = 3,
        };
        await entry.Create(DbContext);

        var stored = await DbContext.ChemicalStockEntries.AsNoTracking().SingleAsync(x => x.Id == entry.Id);
        var version = await DbContext.ChemicalStockEntryVersions.AsNoTracking().SingleAsync(x => x.ChemicalStockEntryId == entry.Id);

        Assert.That(stored.Amount, Is.EqualTo(-1.125m));
        Assert.That(stored.ContainerSize, Is.EqualTo(0.375m));
        Assert.That(stored.Kind, Is.EqualTo(ChemicalStockEntryKindEnum.Consumed));
        Assert.That(version.Amount, Is.EqualTo(-1.125m));
    }

    [Test]
    public async Task ChemicalPropertySettings_SecondRowForSameProperty_IsRejected()
    {
        var property = await CreateProperty();
        await new ChemicalPropertySettings
        {
            PropertyId = property.Id, StockEnabled = true, DigestRecipients = "a@example.com",
            CreatedByUserId = 1, UpdatedByUserId = 1,
        }.Create(DbContext);

        DbContext.ChangeTracker.Clear();
        var duplicate = new ChemicalPropertySettings { PropertyId = property.Id, CreatedByUserId = 1, UpdatedByUserId = 1 };

        Assert.That(async () => await duplicate.Create(DbContext), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task ChemicalWorkerPermission_UniquePerPropertyAndWorker_AndUpdateIsVersioned()
    {
        var property = await CreateProperty();
        var permission = new ChemicalWorkerPermission
        {
            PropertyId = property.Id, WorkerId = 77, View = true, CreatedByUserId = 1, UpdatedByUserId = 1,
        };
        await permission.Create(DbContext);

        permission.Admin = true;
        await permission.Update(DbContext);

        var versions = await DbContext.ChemicalWorkerPermissionVersions.AsNoTracking()
            .Where(x => x.ChemicalWorkerPermissionId == permission.Id).OrderBy(x => x.Version).ToListAsync();
        Assert.That(versions.Select(x => x.Admin), Is.EqualTo(new[] { false, true }));

        DbContext.ChangeTracker.Clear();
        var duplicate = new ChemicalWorkerPermission { PropertyId = property.Id, WorkerId = 77, CreatedByUserId = 1, UpdatedByUserId = 1 };
        Assert.That(async () => await duplicate.Create(DbContext), Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task ChemicalAlertLog_Create_WritesVersion()
    {
        var placement = await CreatePlacement(await CreateLocation(await CreateProperty()));
        var log = new ChemicalAlertLog
        {
            PlacementId = placement.Id, Threshold = ChemicalAlertThresholdEnum.OneMonth,
            DeadlineKind = ChemicalDeadlineKindEnum.UseAndPossession, Channel = ChemicalAlertChannelEnum.Push,
            SentAt = new DateTime(2026, 9, 3, 5, 0, 0, DateTimeKind.Utc), CreatedByUserId = 0, UpdatedByUserId = 0,
        };
        await log.Create(DbContext);

        var version = await DbContext.ChemicalAlertLogVersions.AsNoTracking().SingleAsync(x => x.ChemicalAlertLogId == log.Id);
        Assert.That(version.Threshold, Is.EqualTo(ChemicalAlertThresholdEnum.OneMonth));
        Assert.That(version.DeadlineKind, Is.EqualTo(ChemicalDeadlineKindEnum.UseAndPossession));
        Assert.That(version.Status, Is.Null);
        Assert.That(version.WorkflowState, Is.EqualTo(Constants.WorkflowStates.Created));
    }
}
