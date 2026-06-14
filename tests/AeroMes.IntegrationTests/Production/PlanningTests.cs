using System.Net;
using System.Net.Http.Json;
using AeroMes.Domain.Production;

namespace AeroMes.IntegrationTests.Production;

/// <summary>
/// Integration tests for MPS / MRP production planning — closes #263.
/// </summary>
[Collection("Integration")]
public class PlanningTests(AeroMesWebFactory factory)
{
    // ── MPS Auth guard ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMasterPlans_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/production-plans/master");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── MPS CRUD ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateMasterPlan_ValidPlan_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateMasterPlanAsync(client);
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var id = await resp.Content.ReadFromJsonAsync<int>();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetMasterPlans_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/production-plans/master");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetMasterPlanDetail_AfterCreate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateMasterPlanAsync(client);
        var id = await createResp.Content.ReadFromJsonAsync<int>();

        var resp = await client.GetAsync($"/api/v1/production-plans/master/{id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetMasterPlanDetail_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/production-plans/master/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── MRP ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMrpPlans_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/production-plans/material");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task CreateMrpPlan_ValidPlan_Returns200WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/production-plans/material", new
        {
            planNumber = $"MRP-{Guid.NewGuid():N}"[..12],
            planName = "Integration Test MRP",
            masterPlanId = (int?)null,
            organizationalUnit = (string?)null,
            periodStart = DateOnly.FromDateTime(DateTime.UtcNow),
            periodEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            notes = "Integration test"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task GetMrpPlans_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/production-plans/material");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Detailed Plan ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetDetailedPlans_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/production-plans/detailed");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetDetailedPlans_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/production-plans/detailed");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreateMasterPlanAsync(HttpClient client)
        => client.PostAsJsonAsync("/api/v1/production-plans/master", new
        {
            planNumber = (string?)null,
            planName = $"MPS Integration Test {Guid.NewGuid():N}"[..30],
            organizationalUnit = (string?)null,
            granularity = MpsGranularity.Month,
            periodStart = DateOnly.FromDateTime(DateTime.UtcNow),
            periodEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            dataSource = MpsDataSource.DemandForecast,
            workingHoursPerDay = 8m,
            workingDaysPerWeek = 5,
            lines = Array.Empty<object>()
        });
}
