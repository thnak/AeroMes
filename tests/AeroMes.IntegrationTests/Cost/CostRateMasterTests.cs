using System.Net;
using System.Net.Http.Json;

namespace AeroMes.IntegrationTests.Cost;

/// <summary>
/// Integration tests for the Cost Rate Master data — closes #255.
/// </summary>
[Collection("Integration")]
public class CostRateMasterTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLaborGrades_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/cost/labor-grades");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Labor Grades ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpsertLaborGrade_ValidGrade_Returns200WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/cost/labor-grades", new
        {
            gradeCode = $"GR-{Guid.NewGuid():N}"[..10],
            gradeName = "Senior Technician",
            hourlyRate = 85000m,
            overtimeMultiplier = 1.5m,
            effectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            currency = "VND"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var id = await resp.Content.ReadFromJsonAsync<int>();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetLaborGrades_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/labor-grades");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Machine Cost Rates ────────────────────────────────────────────────

    [Fact]
    public async Task UpsertMachineCostRate_ValidRate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var machineCode = "MCH-COST-01";
        var resp = await client.PostAsJsonAsync($"/api/v1/cost/machines/{machineCode}/cost-rates", new
        {
            ratePerHour = 200000m,
            depreciation = 50000m,
            maintenanceCost = 30000m,
            effectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            currency = "VND"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task GetMachineCostRates_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/machines/MCH-COST-01/cost-rates");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetMachineTotalRate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/machines/MCH-COST-01/total-rate");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Energy Tariffs ────────────────────────────────────────────────────

    [Fact]
    public async Task GetEnergyTariffs_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/energy-tariffs");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task CreateEnergyTariff_ValidTariff_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/cost/energy-tariffs", new
        {
            tariffCode = $"TARIFF-{Guid.NewGuid():N}"[..12],
            tariffName = "Peak Hours",
            ratePerKwh = 3200m,
            effectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            currency = "VND",
            peakFrom = (TimeOnly?)new TimeOnly(8, 0),
            peakTo = (TimeOnly?)new TimeOnly(17, 0)
        });
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Item Standard Cost ────────────────────────────────────────────────

    [Fact]
    public async Task GetItemActiveCost_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/products/MAT-001/active-cost");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetItemCostHistory_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/cost/products/MAT-001/cost-history");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}
