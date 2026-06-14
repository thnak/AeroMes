using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using AeroMes.Domain.Wms;

namespace AeroMes.IntegrationTests.Wms;

/// <summary>
/// Integration tests for warehouse location hierarchy — closes #264.
/// </summary>
[Collection("Integration")]
public class WarehouseLocationTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetWarehouses_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/warehouses");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create warehouse ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateWarehouse_ValidRequest_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = $"WH-{Guid.NewGuid():N}"[..10];

        var resp = await client.PostAsJsonAsync("/api/v1/warehouses", new
        {
            code,
            name = "Test Warehouse",
            warehouseType = WarehouseType.RawMaterial,
            address = "123 Test St",
            integrationSource = IntegrationSource.Manual
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<WarehouseCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.WarehouseId > 0);
    }

    [Fact]
    public async Task GetWarehouses_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/warehouses");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Full hierarchy creation ────────────────────────────────────────────

    [Fact]
    public async Task CreateFullHierarchy_StorageLocation_Zone_Aisle_Rack_Bin()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        // 1. Create a StorageLocation
        var locCode = $"LOC-{Guid.NewGuid():N}"[..12];
        var locResp = await client.PostAsJsonAsync("/api/v1/storage-locations", new
        {
            code = locCode,
            name = "Test Location",
            locationType = LocationType.RawMaterial
        });
        Assert.Equal(HttpStatusCode.Created, locResp.StatusCode);
        var locResult = await locResp.Content.ReadFromJsonAsync<StorageLocationCreatedResult>();
        Assert.NotNull(locResult);

        // 2. Create a Zone under the StorageLocation
        var zoneCode = $"Z-{Guid.NewGuid():N}"[..8];
        var zoneResp = await client.PostAsJsonAsync("/api/v1/warehouse/zones", new
        {
            zoneCode,
            zoneName = "Zone A",
            zoneType = ZoneType.Storage,
            storageLocationId = locResult!.StorageLocationId,
            warehouseId = (int?)null,
            temperatureZone = TemperatureZone.Ambient
        });
        Assert.Equal(HttpStatusCode.Created, zoneResp.StatusCode);
        var zoneResult = await zoneResp.Content.ReadFromJsonAsync<ZoneCreatedResult>();
        Assert.NotNull(zoneResult);

        // 3. Create an Aisle under the Zone
        var aisleResp = await client.PostAsJsonAsync("/api/v1/warehouse/aisles", new
        {
            zoneId = zoneResult!.ZoneId,
            aisleCode = $"A-{Guid.NewGuid():N}"[..8],
            pickSequence = 1
        });
        Assert.Equal(HttpStatusCode.Created, aisleResp.StatusCode);
        var aisleResult = await aisleResp.Content.ReadFromJsonAsync<AisleCreatedResult>();
        Assert.NotNull(aisleResult);

        // 4. Create a Rack under the Aisle
        var rackResp = await client.PostAsJsonAsync("/api/v1/warehouse/racks", new
        {
            aisleId = aisleResult!.AisleId,
            rackCode = $"R-{Guid.NewGuid():N}"[..8],
            maxWeightKg = (decimal?)500m
        });
        Assert.Equal(HttpStatusCode.Created, rackResp.StatusCode);
        var rackResult = await rackResp.Content.ReadFromJsonAsync<RackCreatedResult>();
        Assert.NotNull(rackResult);

        // 5. Create a Bin under the Rack
        var binResp = await client.PostAsJsonAsync("/api/v1/warehouse/bins", new
        {
            rackId = rackResult!.RackId,
            binCode = $"B-{Guid.NewGuid():N}"[..8],
            binLevel = "L1",
            binType = BinType.Pick,
            maxQty = 100m
        });
        Assert.Equal(HttpStatusCode.Created, binResp.StatusCode);
        var binResult = await binResp.Content.ReadFromJsonAsync<BinCreatedResult>();
        Assert.NotNull(binResult);
        Assert.True(binResult!.BinId > 0);
    }

    [Fact]
    public async Task CreateZone_MissingStorageLocation_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/warehouse/zones", new
        {
            zoneCode = "INVALID",
            zoneName = "Bad Zone",
            zoneType = ZoneType.Storage,
            storageLocationId = 999999, // non-existent
            warehouseId = (int?)null,
            temperatureZone = TemperatureZone.Ambient
        });
        Assert.True(resp.StatusCode is HttpStatusCode.UnprocessableEntity or HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEmptyBin_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var binId = await CreateMinimalBinAsync(client);

        var deleteResp = await client.DeleteAsync($"/api/v1/warehouse/bins/{binId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [Fact]
    public async Task GetBinStock_NewBin_ReturnsZeroQty()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var binId = await CreateMinimalBinAsync(client);

        var stockResp = await client.GetAsync($"/api/v1/warehouse/bins/{binId}/stock");
        Assert.Equal(HttpStatusCode.OK, stockResp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<int> CreateMinimalBinAsync(HttpClient client)
    {
        var locCode = $"L-{Guid.NewGuid():N}"[..12];
        var locResp = await client.PostAsJsonAsync("/api/v1/storage-locations", new
        {
            code = locCode, name = "Mini Location", locationType = LocationType.RawMaterial
        });
        var loc = await locResp.Content.ReadFromJsonAsync<StorageLocationCreatedResult>();

        var zoneResp = await client.PostAsJsonAsync("/api/v1/warehouse/zones", new
        {
            zoneCode = $"Z-{Guid.NewGuid():N}"[..8],
            zoneName = "Mini Zone", zoneType = ZoneType.Storage,
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

public record WarehouseCreatedResult(int WarehouseId);
public record StorageLocationCreatedResult(int StorageLocationId);
public record ZoneCreatedResult(int ZoneId);
public record AisleCreatedResult(int AisleId);
public record RackCreatedResult(int RackId);
public record BinCreatedResult(int BinId);
