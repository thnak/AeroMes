using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;

namespace AeroMes.IntegrationTests.Production;

/// <summary>
/// Integration tests for the Work Order and Job lifecycle — closes #263.
/// </summary>
[Collection("Integration")]
public class WorkOrderLifecycleTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkOrders_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/work-orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Work Order queries ─────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkOrders_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/work-orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<WorkOrderDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task GetWorkOrderById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/work-orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Job queries ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetJobs_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/jobs");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetJobById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/jobs/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Downtime queries ───────────────────────────────────────────────────

    [Fact]
    public async Task GetDowntimeLogs_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/downtime");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetDowntimeLogs_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/downtime");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Stage handovers ────────────────────────────────────────────────────

    [Fact]
    public async Task GetStageHandovers_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/stage-handovers");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetStageHandovers_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/stage-handovers");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Production statistics ──────────────────────────────────────────────

    [Fact]
    public async Task GetProductionStatistics_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/production-statistics");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Material blends ────────────────────────────────────────────────────

    [Fact]
    public async Task GetMaterialBlends_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/material-blends");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetMaterialBlends_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/material-blends");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}

file record WorkOrderDto(int WOID, string WOCode);
