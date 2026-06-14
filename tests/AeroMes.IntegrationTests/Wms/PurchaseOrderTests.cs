using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreatePurchaseOrder;
using AeroMes.Application.Wms.Queries.GetPurchaseOrders;
using AeroMes.Domain.Master;
using AeroMes.Domain.Wms;

namespace AeroMes.IntegrationTests.Wms;

/// <summary>
/// Integration tests for Purchase Order lifecycle — closes #265.
/// </summary>
[Collection("Integration")]
public class PurchaseOrderTests(AeroMesWebFactory factory)
{
    private static string NewPoCode() => $"PO-{Guid.NewGuid():N}"[..15];

    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPurchaseOrders_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/warehouse/purchase-orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create PO ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePurchaseOrder_ValidRequest_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreatePoAsync(client, NewPoCode(), 10m);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<PoCreatedResult>>();
        Assert.NotNull(body?.Data);
        Assert.True(body!.Data!.PoId > 0);
    }

    [Fact]
    public async Task CreatePurchaseOrder_ZeroQuantityLine_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/warehouse/purchase-orders", new
        {
            poCode = NewPoCode(),
            supplierCode = "SUP-001",
            expectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            lines = new[] { new { productCode = "MAT-001", orderedQty = 0m, unitPrice = (decimal?)null, expectedLotNumber = (string?)null } },
            notes = (string?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task CreatePurchaseOrder_PastDeliveryDate_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/warehouse/purchase-orders", new
        {
            poCode = NewPoCode(),
            supplierCode = "SUP-001",
            expectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            lines = new[] { new { productCode = "MAT-001", orderedQty = 10m, unitPrice = (decimal?)null, expectedLotNumber = (string?)null } },
            notes = (string?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task CreatePurchaseOrder_DuplicatePoCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = NewPoCode();

        await CreatePoAsync(client, code, 10m);
        var dupResp = await CreatePoAsync(client, code, 5m);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, dupResp.StatusCode);
    }

    // ── Confirm PO ────────────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmPurchaseOrder_DraftPo_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePoAsync(client, NewPoCode(), 50m);
        var result = await createResp.Content.ReadFromJsonAsync<ApiResponse<PoCreatedResult>>();
        var poId = result!.Data!.PoId;

        var confirmResp = await client.PostAsync($"/api/v1/warehouse/purchase-orders/{poId}/confirm", null);
        Assert.Equal(HttpStatusCode.NoContent, confirmResp.StatusCode);
    }

    [Fact]
    public async Task ConfirmPurchaseOrder_NonExistent_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsync("/api/v1/warehouse/purchase-orders/999999/confirm", null);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Query ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPurchaseOrders_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/warehouse/purchase-orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PurchaseOrderDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task GetPurchaseOrders_FilterByStatus_OnlyReturnsDraftPOs()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        // Create a draft PO
        await CreatePoAsync(client, NewPoCode(), 20m);

        var resp = await client.GetAsync("/api/v1/warehouse/purchase-orders?status=Draft");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PurchaseOrderDto>>>();
        Assert.NotNull(body?.Data);
        Assert.All(body!.Data!, po => Assert.Equal("Draft", po.Status));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreatePoAsync(HttpClient client, string code, decimal qty)
        => client.PostAsJsonAsync("/api/v1/warehouse/purchase-orders", new
        {
            poCode = code,
            supplierCode = "SUP-001",
            expectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            lines = new[] { new { productCode = "MAT-001", orderedQty = qty, unitPrice = (decimal?)null, expectedLotNumber = (string?)null } },
            notes = (string?)null
        });
}
