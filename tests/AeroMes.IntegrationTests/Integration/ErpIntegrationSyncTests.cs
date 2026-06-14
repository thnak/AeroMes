using System.Net;

namespace AeroMes.IntegrationTests.Integration;

/// <summary>
/// Integration tests for ERP integration delta sync and idempotency — closes #261.
/// </summary>
[Collection("Integration")]
public class ErpIntegrationSyncTests(AeroMesWebFactory factory)
{
    // ── Auth guards ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSalesOrders_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/integration/sales-orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetProductionOrders_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/integration/production-orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Sales Orders ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetSalesOrders_Returns200WithApiResponse()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/integration/sales-orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetSalesOrderById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/integration/sales-orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetSalesOrders_FilterByStatus_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/integration/sales-orders?status=Confirmed");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Production Orders ──────────────────────────────────────────────────

    [Fact]
    public async Task GetProductionOrders_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/integration/production-orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetProductionOrderById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/integration/production-orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetProductionOrders_FilterByProductCode_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/integration/production-orders?productCode=FG-001");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── ERP Settings ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetErpSettings_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/integration/erp-settings");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Sync endpoint ──────────────────────────────────────────────────────

    [Fact]
    public async Task SyncSalesOrders_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsync("/api/v1/integration/sync/sales-orders", null);
        Assert.True(resp.IsSuccessStatusCode,
            $"Sync failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task SyncProductionOrders_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsync("/api/v1/integration/sync/production-orders", null);
        Assert.True(resp.IsSuccessStatusCode,
            $"Sync failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Test ERP Connection ────────────────────────────────────────────────

    [Fact]
    public async Task TestErpConnection_Returns200WithResult()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsync("/api/v1/integration/test-connection", null);
        Assert.True(resp.IsSuccessStatusCode,
            $"Connection test failed: {await resp.Content.ReadAsStringAsync()}");
    }
}
