using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Jobs.Queries.GetJobs;

namespace AeroMes.IntegrationTests.Production;

[Collection("Integration")]
public class JobsTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/jobs");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/jobs/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/jobs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<JobDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/jobs/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
