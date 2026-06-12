using System.Net;

namespace AeroMes.IntegrationTests.Production;

[Collection("Integration")]
public class WorkOrderDetailTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task GetById_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/work-orders/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/work-orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
