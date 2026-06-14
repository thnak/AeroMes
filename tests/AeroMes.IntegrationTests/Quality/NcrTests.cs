using System.Net;
using System.Net.Http.Json;

namespace AeroMes.IntegrationTests.Quality;

/// <summary>
/// Integration tests for the NCR (Non-Conformance Report) lifecycle — closes #259.
/// </summary>
[Collection("Integration")]
public class NcrTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetNcrs_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/quality/ncrs");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create NCR ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateManualNcr_ValidPayload_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateNcrAsync(client);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<NcrCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.NcrId > 0);
    }

    [Fact]
    public async Task CreateManualNcr_ZeroQty_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/quality/ncrs", new
        {
            workOrderId = 1L,
            productCode = "FG-001",
            lotNumber = (string?)null,
            qtyAffected = 0m,
            severity = "MAJOR"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    // ── Get NCRs ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetNcrs_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/quality/ncrs");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetNcrDetail_AfterCreate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateNcrAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<NcrCreatedResult>();

        var resp = await client.GetAsync($"/api/v1/quality/ncrs/{result!.NcrId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetNcrDetail_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/quality/ncrs/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── NCR lifecycle ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateNcr_AssignTo_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateNcrAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<NcrCreatedResult>();

        var resp = await client.PutAsJsonAsync($"/api/v1/quality/ncrs/{result!.NcrId}", new
        {
            assignedTo = "QC-MANAGER-001",
            dueDate = (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(7)
        });
        Assert.True(resp.IsSuccessStatusCode, $"Update NCR failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task SetDisposition_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateNcrAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<NcrCreatedResult>();

        var resp = await client.PostAsJsonAsync($"/api/v1/quality/ncrs/{result!.NcrId}/disposition", new
        {
            dispositionCode = "REWORK",
            setBy = "QC-001"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Set disposition failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task CloseNcr_WithResolution_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateNcrAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<NcrCreatedResult>();

        var resp = await client.PostAsJsonAsync($"/api/v1/quality/ncrs/{result!.NcrId}/close", new
        {
            closedBy = "QC-MGR-001",
            resolutionDescription = "Root cause identified and corrective action applied.",
            eSignatureToken = "test-token"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Close NCR failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task CancelNcr_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateNcrAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<NcrCreatedResult>();

        var resp = await client.PostAsJsonAsync($"/api/v1/quality/ncrs/{result!.NcrId}/cancel", new
        {
            cancelledBy = "QC-001"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Cancel NCR failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreateNcrAsync(HttpClient client)
        => client.PostAsJsonAsync("/api/v1/quality/ncrs", new
        {
            workOrderId = 1L,
            productCode = "FG-001",
            lotNumber = $"LOT-{Guid.NewGuid():N}"[..15],
            qtyAffected = 10m,
            severity = "MAJOR"
        });
}

file record NcrCreatedResult(int NcrId);
