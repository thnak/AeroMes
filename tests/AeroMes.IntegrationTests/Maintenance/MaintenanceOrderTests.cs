using System.Net;
using System.Net.Http.Json;
using AeroMes.Domain.Maintenance;

namespace AeroMes.IntegrationTests.Maintenance;

/// <summary>
/// Integration tests for the Maintenance Order lifecycle — closes #256.
/// </summary>
[Collection("Integration")]
public class MaintenanceOrderTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrders_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/maintenance/orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateMaintenanceOrder_Preventive_Returns200WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateOrderAsync(client, MaintenanceOrderType.Preventive, MaintenancePriority.Normal);
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var id = await resp.Content.ReadFromJsonAsync<int>();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task CreateMaintenanceOrder_Corrective_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateOrderAsync(client, MaintenanceOrderType.Corrective, MaintenancePriority.Critical);
        Assert.True(resp.IsSuccessStatusCode);
    }

    [Fact]
    public async Task CreateMaintenanceOrder_Predictive_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateOrderAsync(client, MaintenanceOrderType.Predictive, MaintenancePriority.High);
        Assert.True(resp.IsSuccessStatusCode);
    }

    [Fact]
    public async Task CreateMaintenanceOrder_Calibration_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateOrderAsync(client, MaintenanceOrderType.Calibration, MaintenancePriority.Low);
        Assert.True(resp.IsSuccessStatusCode);
    }

    [Fact]
    public async Task CreateMaintenanceOrder_EmptyCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/maintenance/orders", new
        {
            maintOrderCode = "",
            machineCode = "MCH-001",
            orderType = MaintenanceOrderType.Preventive,
            priority = MaintenancePriority.Normal
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    // ── Status lifecycle ───────────────────────────────────────────────────

    [Fact]
    public async Task StartOrder_OpenOrder_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "start" });
        Assert.True(resp.IsSuccessStatusCode, $"Start failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task CompleteOrder_AfterStart_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);
        await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "start" });

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "complete" });
        Assert.True(resp.IsSuccessStatusCode, $"Complete failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task HoldForParts_WhileInProgress_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);
        await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "start" });

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "hold" });
        Assert.True(resp.IsSuccessStatusCode, $"Hold failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task CancelOrder_WhenOpen_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "cancel" });
        Assert.True(resp.IsSuccessStatusCode, $"Cancel failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task CompleteOrder_WhenOpen_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "complete" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task InvalidAction_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "fly" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    // ── Cost lines ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddCostLine_Labor_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);
        await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/status", new { action = "start" });

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/cost-lines", new
        {
            costCategory = CostCategory.Labor,
            productCode = (string?)null,
            lotNumber = (string?)null,
            qtyUsed = (decimal?)null,
            unitCost = (decimal?)null,
            operatorID = "OP-001",
            laborHours = 2.5m,
            laborRateSnapshot = 50000m,
            supplierID = (int?)null,
            invoiceRef = (string?)null,
            invoiceAmount = (decimal?)null
        });
        Assert.True(resp.IsSuccessStatusCode, $"Add cost line failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task AddCostLine_Parts_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAndGetIdAsync(client);

        var resp = await client.PostAsJsonAsync($"/api/v1/maintenance/orders/{id}/cost-lines", new
        {
            costCategory = CostCategory.SparePart,
            productCode = "PART-001",
            lotNumber = "LOT-001",
            qtyUsed = 3m,
            unitCost = 150000m,
            operatorID = (string?)null,
            laborHours = (decimal?)null,
            laborRateSnapshot = (decimal?)null,
            supplierID = (int?)null,
            invoiceRef = "INV-2026-001",
            invoiceAmount = 450000m
        });
        Assert.True(resp.IsSuccessStatusCode, $"Add parts cost line failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Queries ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrders_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/maintenance/orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetOrders_FilterByStatus_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/maintenance/orders?status=Open");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetTco_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/maintenance/tco/MCH-TEST?months=6");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetCalendar_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var from = DateTime.UtcNow.AddDays(-30).ToString("O");
        var to = DateTime.UtcNow.AddDays(30).ToString("O");
        var resp = await client.GetAsync($"/api/v1/maintenance/calendar?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreateOrderAsync(
        HttpClient client, MaintenanceOrderType type, MaintenancePriority priority)
        => client.PostAsJsonAsync("/api/v1/maintenance/orders", new
        {
            maintOrderCode = $"MO-{Guid.NewGuid():N}"[..20],
            machineCode = "MCH-TEST-001",
            orderType = type,
            triggerRef = (string?)null,
            priority,
            plannedStartAt = (DateTime?)DateTime.UtcNow.AddDays(1),
            plannedEndAt = (DateTime?)DateTime.UtcNow.AddDays(2),
            assignedTo = "TECH-001",
            estimatedCost = (decimal?)500000m,
            notes = "Integration test order"
        });

    private async Task<int> CreateAndGetIdAsync(HttpClient client)
    {
        var resp = await CreateOrderAsync(client, MaintenanceOrderType.Preventive, MaintenancePriority.Normal);
        return await resp.Content.ReadFromJsonAsync<int>();
    }
}
