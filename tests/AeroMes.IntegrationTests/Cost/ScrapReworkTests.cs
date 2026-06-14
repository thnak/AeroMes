using System.Net;
using System.Net.Http.Json;
using AeroMes.Domain.Cost;

namespace AeroMes.IntegrationTests.Cost;

/// <summary>
/// Integration tests for Scrap posting and Rework Order lifecycle — closes #255.
/// </summary>
[Collection("Integration")]
public class ScrapReworkTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetReworkOrders_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/cost/rework-orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Scrap posting ──────────────────────────────────────────────────────

    [Fact]
    public async Task PostScrap_ValidPayload_Returns200WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await PostScrapAsync(client);
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var id = await resp.Content.ReadFromJsonAsync<long>();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task PostScrap_ZeroQty_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/cost/scrap", new
        {
            woid = 1,
            logID = (long?)null,
            defectCodeId = (int?)null,
            productCode = "FG-001",
            lotNumber = (string?)null,
            scrapQty = 0,
            materialCostPerUnit = 10000m,
            laborCostSunk = 5000m,
            machineCostSunk = 2000m,
            disposalMethod = DisposalMethod.Scrap,
            scrapLocationId = (int?)null,
            approvedBy = (string?)null,
            notes = (string?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task GetScrapPareto_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var from = DateTime.UtcNow.AddDays(-30).ToString("O");
        var to = DateTime.UtcNow.ToString("O");
        var resp = await client.GetAsync($"/api/v1/cost/scrap/pareto?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Rework Orders ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateReworkOrder_ValidPayload_Returns200WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateReworkAsync(client);
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var id = await resp.Content.ReadFromJsonAsync<int>();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task CreateReworkOrder_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = $"RW-{Guid.NewGuid():N}"[..12];

        await client.PostAsJsonAsync("/api/v1/cost/rework-orders", new
        {
            reworkCode = code,
            sourceWOID = 1,
            scrapTxID = (long?)null,
            productCode = "FG-001",
            reworkQty = 5,
            reworkStepFromId = (int?)null
        });

        var resp = await client.PostAsJsonAsync("/api/v1/cost/rework-orders", new
        {
            reworkCode = code,
            sourceWOID = 1,
            scrapTxID = (long?)null,
            productCode = "FG-001",
            reworkQty = 5,
            reworkStepFromId = (int?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task CloseReworkOrder_ValidOrder_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateReworkAsync(client);
        var id = await createResp.Content.ReadFromJsonAsync<int>();

        var resp = await client.PostAsJsonAsync($"/api/v1/cost/rework-orders/{id}/complete", new
        {
            actMaterialCost = 50000m,
            actLaborCost = 80000m,
            actMachineCost = 20000m
        });
        Assert.True(resp.IsSuccessStatusCode, $"Close rework failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task GetReworkOrders_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/rework-orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Quality cost summary ───────────────────────────────────────────────

    [Fact]
    public async Task GetQualityCostSummary_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var year = (short)DateTime.UtcNow.Year;
        var resp = await client.GetAsync($"/api/v1/cost/quality-cost-summary?year={year}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetCopqTrend_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/copq-trend?months=6");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> PostScrapAsync(HttpClient client)
        => client.PostAsJsonAsync("/api/v1/cost/scrap", new
        {
            woid = 1,
            logID = (long?)null,
            defectCodeId = (int?)null,
            productCode = "FG-001",
            lotNumber = $"LOT-{Guid.NewGuid():N}"[..15],
            scrapQty = 3,
            materialCostPerUnit = 10000m,
            laborCostSunk = 5000m,
            machineCostSunk = 2000m,
            disposalMethod = DisposalMethod.Scrap,
            scrapLocationId = (int?)null,
            approvedBy = "QM-001",
            notes = "Integration test scrap"
        });

    private static Task<HttpResponseMessage> CreateReworkAsync(HttpClient client)
        => client.PostAsJsonAsync("/api/v1/cost/rework-orders", new
        {
            reworkCode = $"RW-{Guid.NewGuid():N}"[..12],
            sourceWOID = 1,
            scrapTxID = (long?)null,
            productCode = "FG-001",
            reworkQty = 5,
            reworkStepFromId = (int?)null
        });
}
