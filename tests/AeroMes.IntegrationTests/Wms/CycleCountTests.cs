using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Wms.Commands.CreateCycleCountPlan;
using AeroMes.Domain.Master;
using AeroMes.Domain.Wms;

namespace AeroMes.IntegrationTests.Wms;

/// <summary>
/// Integration tests for the Cycle Count workflow — closes #266.
/// </summary>
[Collection("Integration")]
public class CycleCountTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCycleCounts_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/cycle-counts");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create plan ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCycleCountPlan_Returns201WithPlanId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreatePlanAsync(client);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<CycleCountPlanCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.PlanId > 0);
    }

    [Fact]
    public async Task GetCycleCount_AfterCreate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client);
        var plan = await createResp.Content.ReadFromJsonAsync<CycleCountPlanCreatedResult>();

        var resp = await client.GetAsync($"/api/v1/cycle-counts/{plan!.PlanId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Generate lines ─────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateCycleCountLines_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client);
        var plan = await createResp.Content.ReadFromJsonAsync<CycleCountPlanCreatedResult>();

        var genResp = await client.PostAsJsonAsync(
            $"/api/v1/cycle-counts/{plan!.PlanId}/generate-lines",
            new { binIds = (int[]?)null }); // null = all bins in scope

        Assert.True(genResp.IsSuccessStatusCode,
            $"Expected success, got {genResp.StatusCode}: {await genResp.Content.ReadAsStringAsync()}");
    }

    // ── Get sheet ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCycleCountSheet_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client);
        var plan = await createResp.Content.ReadFromJsonAsync<CycleCountPlanCreatedResult>();
        await client.PostAsJsonAsync($"/api/v1/cycle-counts/{plan!.PlanId}/generate-lines", new { binIds = (int[]?)null });

        var sheetResp = await client.GetAsync($"/api/v1/cycle-counts/{plan.PlanId}/sheet");
        Assert.Equal(HttpStatusCode.OK, sheetResp.StatusCode);
    }

    // ── Submit ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitCycleCount_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client);
        var plan = await createResp.Content.ReadFromJsonAsync<CycleCountPlanCreatedResult>();
        await client.PostAsJsonAsync($"/api/v1/cycle-counts/{plan!.PlanId}/generate-lines", new { binIds = (int[]?)null });

        var submitResp = await client.PostAsync($"/api/v1/cycle-counts/{plan.PlanId}/submit", null);
        Assert.True(submitResp.IsSuccessStatusCode,
            $"Expected success, got {submitResp.StatusCode}: {await submitResp.Content.ReadAsStringAsync()}");
    }

    // ── List plans ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCycleCounts_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cycle-counts");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetCycleCount_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cycle-counts/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Helper ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreatePlanAsync(HttpClient client)
        => client.PostAsJsonAsync("/api/v1/cycle-counts", new
        {
            planType = CycleCountPlanType.Full,
            scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            notes = "Integration test cycle count"
        });
}
