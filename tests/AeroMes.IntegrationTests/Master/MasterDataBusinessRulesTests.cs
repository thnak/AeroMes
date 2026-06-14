using System.Net;
using System.Net.Http.Json;

namespace AeroMes.IntegrationTests.Master;

/// <summary>
/// Integration tests for complex Master Data business rules — closes #258.
/// </summary>
[Collection("Integration")]
public class MasterDataBusinessRulesTests(AeroMesWebFactory factory)
{
    // ── BOM versioning ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetBomVersions_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/bom/FG-001/versions");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task CreateBomDraft_Returns200WithVersionId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var productCode = $"BOM-{Guid.NewGuid():N}"[..10];

        var resp = await client.PostAsJsonAsync($"/api/v1/bom/{productCode}/versions", new
        {
            version = "1.0",
            effectiveFrom = (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow),
            notes = "Integration test BOM draft",
            lines = Array.Empty<object>()
        });
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var result = await resp.Content.ReadFromJsonAsync<BomDraftCreatedResult>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetBomVersions_AfterCreate_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var productCode = $"BVER-{Guid.NewGuid():N}"[..10];
        await client.PostAsJsonAsync($"/api/v1/bom/{productCode}/versions", new
        {
            version = "1.0",
            effectiveFrom = (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow),
            notes = "Test",
            lines = Array.Empty<object>()
        });

        var resp = await client.GetAsync($"/api/v1/bom/{productCode}/versions");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetActiveBom_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/bom/FG-001");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Routing ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRoutings_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/routings");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task CreateRouting_ValidRouting_Returns200WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/routings", new
        {
            code = $"RT-{Guid.NewGuid():N}"[..12],
            name = "Test Routing",
            productCode = "FG-001",
            isDefault = false
        });
        Assert.True(resp.IsSuccessStatusCode, $"Got {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var result = await resp.Content.ReadFromJsonAsync<RoutingCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.RoutingId > 0);
    }

    [Fact]
    public async Task CreateRouting_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = $"RT-{Guid.NewGuid():N}"[..12];

        await client.PostAsJsonAsync("/api/v1/routings", new
        {
            code, name = "First", productCode = "FG-001", isDefault = false
        });

        var dupResp = await client.PostAsJsonAsync("/api/v1/routings", new
        {
            code, name = "Duplicate", productCode = "FG-001", isDefault = false
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, dupResp.StatusCode);
    }

    [Fact]
    public async Task GetRoutings_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/routings");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Work Calendars ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkCalendars_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/work-calendars");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Machines ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMachines_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/machines");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetMachines_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/machines");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Work Centers ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkCenters_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/work-centers");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Operations ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOperations_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/operations");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Customers ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCustomers_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/customers");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Products ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/products");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}

file record BomDraftCreatedResult(int BomHeaderId, string Version);
file record RoutingCreatedResult(int RoutingId);
