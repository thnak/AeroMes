using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Inventory.Queries.GetInventoryStock;
using AeroMes.Application.Inventory.Queries.GetLotTrace;

namespace AeroMes.IntegrationTests.Inventory;

[Collection("Integration")]
public class InventoryTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task GetStock_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/inventory");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TraceLot_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/inventory/trace/LOT001");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStock_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/inventory");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<InventoryStockDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task TraceLot_UnknownLot_Returns200WithEmptyList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/inventory/trace/UNKNOWN-LOT-999999");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LotTraceDto>>();
        Assert.NotNull(body?.Data);
        Assert.Empty(body!.Data!.StockEntries);
    }
}
