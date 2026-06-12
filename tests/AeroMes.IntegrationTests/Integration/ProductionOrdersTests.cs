using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Integration.Queries.GetProductionOrders;

namespace AeroMes.IntegrationTests.Integration;

[Collection("Integration")]
public class ProductionOrdersTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/integration/production-orders");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/integration/production-orders/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/integration/production-orders");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ProductionOrderDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/integration/production-orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
