using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.AddGrnLine;
using AeroMes.Application.Wms.Commands.CreateGrn;
using AeroMes.Application.Wms.Commands.CreatePurchaseOrder;
using AeroMes.Application.Wms.Queries.GetGrnList;
using AeroMes.Domain.Master;
using AeroMes.Domain.Wms;

namespace AeroMes.IntegrationTests.Wms;

/// <summary>
/// Integration tests for GRN (Goods Receipt Note) lifecycle — closes #270.
/// </summary>
[Collection("Integration")]
public class GrnTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGrns_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/warehouse/grn");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGrn_WithValidPoAndBin_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (poId, binId) = await SeedPoAndBinAsync(client, 50m);

        var grnResp = await CreateGrnAsync(client, poId, binId);
        Assert.Equal(HttpStatusCode.Created, grnResp.StatusCode);
        var body = await grnResp.Content.ReadFromJsonAsync<ApiResponse<GrnCreatedResult>>();
        Assert.NotNull(body?.Data);
        Assert.True(body!.Data!.GrnId > 0);
    }

    [Fact]
    public async Task AddGrnLine_ToExistingGrn_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (poId, binId) = await SeedPoAndBinAsync(client, 100m);

        var grnResp = await CreateGrnAsync(client, poId, binId);
        var grn = (await grnResp.Content.ReadFromJsonAsync<ApiResponse<GrnCreatedResult>>())!.Data!;

        var lineResp = await client.PostAsJsonAsync($"/api/v1/warehouse/grn/{grn.GrnId}/lines", new
        {
            poLineId = (int?)null,
            productCode = "MAT-001",
            lotNumber = $"LOT-{Guid.NewGuid():N}"[..15],
            receivedQty = 30m,
            manufacturedDate = (DateOnly?)null,
            expiryDate = (DateOnly?)null,
            grossWeightKg = (decimal?)null,
            destinationBinId = binId
        });
        Assert.Equal(HttpStatusCode.Created, lineResp.StatusCode);
        var lineResult = await lineResp.Content.ReadFromJsonAsync<ApiResponse<GrnLineAddedResult>>();
        Assert.NotNull(lineResult?.Data);
    }

    [Fact]
    public async Task ConfirmGrn_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (poId, binId) = await SeedPoAndBinAsync(client, 20m);

        var grnResp = await CreateGrnAsync(client, poId, binId);
        var grn = (await grnResp.Content.ReadFromJsonAsync<ApiResponse<GrnCreatedResult>>())!.Data!;

        // Add at least one line
        await client.PostAsJsonAsync($"/api/v1/warehouse/grn/{grn.GrnId}/lines", new
        {
            poLineId = (int?)null,
            productCode = "MAT-001",
            lotNumber = $"LOT-{Guid.NewGuid():N}"[..15],
            receivedQty = 20m,
            manufacturedDate = (DateOnly?)null,
            expiryDate = (DateOnly?)null,
            grossWeightKg = (decimal?)null,
            destinationBinId = binId
        });

        var confirmResp = await client.PostAsync($"/api/v1/warehouse/grn/{grn.GrnId}/confirm", null);
        Assert.Equal(HttpStatusCode.NoContent, confirmResp.StatusCode);
    }

    [Fact]
    public async Task GetGrnList_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/warehouse/grn");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<GrnListDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task GetGrnById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/warehouse/grn/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<(int PoId, int BinId)> SeedPoAndBinAsync(HttpClient client, decimal qty)
    {
        // Create and confirm a PO
        var poCode = $"PO-{Guid.NewGuid():N}"[..15];
        var poResp = await client.PostAsJsonAsync("/api/v1/warehouse/purchase-orders", new
        {
            poCode,
            supplierCode = "SUP-001",
            expectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            lines = new[] { new { productCode = "MAT-001", orderedQty = qty, unitPrice = (decimal?)null, expectedLotNumber = (string?)null } },
            notes = (string?)null
        });
        var poResult = (await poResp.Content.ReadFromJsonAsync<ApiResponse<PoCreatedResult>>())!.Data!;
        await client.PostAsync($"/api/v1/warehouse/purchase-orders/{poResult.PoId}/confirm", null);

        // Create a bin
        var binId = await CreateMinimalBinAsync(client);
        return (poResult.PoId, binId);
    }

    private async Task<HttpResponseMessage> CreateGrnAsync(HttpClient client, int poId, int binId)
    {
        return await client.PostAsJsonAsync("/api/v1/warehouse/grn", new
        {
            grnCode = $"GRN-{Guid.NewGuid():N}"[..15],
            poId = (int?)poId,
            storageLocationId = 1, // will be overridden by actual location
            receivedBy = "test-user",
            receivedAt = DateTime.UtcNow,
            deliveryNoteRef = "DN-001",
            notes = (string?)null
        });
    }

    private async Task<int> CreateMinimalBinAsync(HttpClient client)
    {
        var locCode = $"L-{Guid.NewGuid():N}"[..12];
        var locResp = await client.PostAsJsonAsync("/api/v1/storage-locations", new
        {
            code = locCode, name = "GRN Location", locationType = LocationType.RawMaterial
        });
        var loc = await locResp.Content.ReadFromJsonAsync<StorageLocationCreatedResult>();

        var zoneResp = await client.PostAsJsonAsync("/api/v1/warehouse/zones", new
        {
            zoneCode = $"Z-{Guid.NewGuid():N}"[..8],
            zoneName = "GRN Zone", zoneType = ZoneType.Receiving,
            storageLocationId = loc!.StorageLocationId, warehouseId = (int?)null,
            temperatureZone = TemperatureZone.Ambient
        });
        var zone = await zoneResp.Content.ReadFromJsonAsync<ZoneCreatedResult>();

        var aisleResp = await client.PostAsJsonAsync("/api/v1/warehouse/aisles", new
        {
            zoneId = zone!.ZoneId, aisleCode = $"A-{Guid.NewGuid():N}"[..8], pickSequence = 1
        });
        var aisle = await aisleResp.Content.ReadFromJsonAsync<AisleCreatedResult>();

        var rackResp = await client.PostAsJsonAsync("/api/v1/warehouse/racks", new
        {
            aisleId = aisle!.AisleId, rackCode = $"R-{Guid.NewGuid():N}"[..8], maxWeightKg = (decimal?)null
        });
        var rack = await rackResp.Content.ReadFromJsonAsync<RackCreatedResult>();

        var binResp = await client.PostAsJsonAsync("/api/v1/warehouse/bins", new
        {
            rackId = rack!.RackId, binCode = $"B-{Guid.NewGuid():N}"[..8],
            binLevel = "L1", binType = BinType.Bulk, maxQty = (decimal?)null
        });
        var bin = await binResp.Content.ReadFromJsonAsync<BinCreatedResult>();
        return bin!.BinId;
    }
}
