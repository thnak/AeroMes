using System.Net;
using System.Net.Http.Json;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;

namespace AeroMes.IntegrationTests.Traceability;

/// <summary>
/// Integration tests for the recall lifecycle — closes #268.
/// </summary>
[Collection("Integration")]
public class RecallTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task InitiateRecall_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().PostAsJsonAsync("/api/v1/recalls", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ListRecalls_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/recalls");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Initiate recall ────────────────────────────────────────────────────

    [Fact]
    public async Task InitiateRecall_ValidRequest_Returns201WithGuid()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await InitiateRecallAsync(client, $"RL-{Guid.NewGuid():N}"[..20]);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task InitiateRecall_ForwardDirection_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/recalls", new
        {
            title = "Forward Recall Test",
            recallType = RecallType.SupplierAlert,
            anchorLotNumber = $"FW-{Guid.NewGuid():N}"[..18],
            anchorDirection = AnchorDirection.Forward,
            description = (string?)null,
            regulatoryRef = (string?)null
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task InitiateRecall_BidirectionalDirection_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/recalls", new
        {
            title = "Bidirectional Recall Test",
            recallType = RecallType.InternalDetection,
            anchorLotNumber = $"BI-{Guid.NewGuid():N}"[..18],
            anchorDirection = AnchorDirection.Bidirectional,
            description = "Detected in process",
            regulatoryRef = (string?)null
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    // ── Identify scope ─────────────────────────────────────────────────────

    [Fact]
    public async Task IdentifyScope_AfterRecallInitiated_Returns200WithScope()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"SC-{Guid.NewGuid():N}"[..18];

        // Seed a multi-hop lineage so scope is non-trivial
        await RecordLineageAsync(client, lot, $"SC-W-{Guid.NewGuid():N}"[..18], LineageType.Consume, 50m);

        var recallResp = await InitiateRecallAsync(client, lot);
        var recallId = await recallResp.Content.ReadFromJsonAsync<Guid>();

        var scopeResp = await client.PostAsync($"/api/v1/recalls/{recallId}/identify-scope", null);
        Assert.Equal(HttpStatusCode.OK, scopeResp.StatusCode);
        var scope = await scopeResp.Content.ReadFromJsonAsync<RecallScopeDto>();
        Assert.NotNull(scope);
    }

    [Fact]
    public async Task GetScope_AfterIdentify_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"GS-{Guid.NewGuid():N}"[..18];

        var recallResp = await InitiateRecallAsync(client, lot);
        var recallId = await recallResp.Content.ReadFromJsonAsync<Guid>();
        await client.PostAsync($"/api/v1/recalls/{recallId}/identify-scope", null);

        var resp = await client.GetAsync($"/api/v1/recalls/{recallId}/scope");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Apply quarantine ───────────────────────────────────────────────────

    [Fact]
    public async Task ApplyQuarantine_AfterScopeIdentified_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"QR-{Guid.NewGuid():N}"[..18];

        var recallResp = await InitiateRecallAsync(client, lot);
        var recallId = await recallResp.Content.ReadFromJsonAsync<Guid>();
        await client.PostAsync($"/api/v1/recalls/{recallId}/identify-scope", null);

        var qResp = await client.PostAsync($"/api/v1/recalls/{recallId}/apply-quarantine", null);
        Assert.True(qResp.IsSuccessStatusCode, $"Expected success, got {qResp.StatusCode}");
    }

    // ── Close recall ───────────────────────────────────────────────────────

    [Fact]
    public async Task CloseRecall_WithResolutionNote_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"CR-{Guid.NewGuid():N}"[..18];

        var recallResp = await InitiateRecallAsync(client, lot);
        var recallId = await recallResp.Content.ReadFromJsonAsync<Guid>();
        await client.PostAsync($"/api/v1/recalls/{recallId}/identify-scope", null);

        var closeResp = await client.PostAsJsonAsync($"/api/v1/recalls/{recallId}/close", new
        {
            eSignatureToken = "test-signature",
            closureNotes = "All lots accounted for, no further action required."
        });
        Assert.Equal(HttpStatusCode.NoContent, closeResp.StatusCode);
    }

    [Fact]
    public async Task GetRecall_AfterClose_ShowsClosedStatus()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"RC-{Guid.NewGuid():N}"[..18];

        var recallResp = await InitiateRecallAsync(client, lot);
        var recallId = await recallResp.Content.ReadFromJsonAsync<Guid>();
        await client.PostAsync($"/api/v1/recalls/{recallId}/identify-scope", null);
        await client.PostAsJsonAsync($"/api/v1/recalls/{recallId}/close", new
        {
            eSignatureToken = "test-sig",
            closureNotes = "Closed."
        });

        var getResp = await client.GetAsync($"/api/v1/recalls/{recallId}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var detail = await getResp.Content.ReadAsStringAsync();
        Assert.Contains("Closed", detail, StringComparison.OrdinalIgnoreCase);
    }

    // ── List & audit ───────────────────────────────────────────────────────

    [Fact]
    public async Task ListRecalls_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/recalls");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAuditLog_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var recallResp = await InitiateRecallAsync(client, $"AL-{Guid.NewGuid():N}"[..18]);
        var recallId = await recallResp.Content.ReadFromJsonAsync<Guid>();

        var resp = await client.GetAsync($"/api/v1/recalls/{recallId}/audit-log");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetRecall_UnknownId_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync($"/api/v1/recalls/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> InitiateRecallAsync(HttpClient client, string anchorLot)
        => client.PostAsJsonAsync("/api/v1/recalls", new
        {
            title = $"Recall for {anchorLot}",
            recallType = RecallType.SupplierAlert,
            anchorLotNumber = anchorLot,
            anchorDirection = AnchorDirection.Backward,
            description = "Integration test recall",
            regulatoryRef = (string?)null
        });

    private static async Task RecordLineageAsync(
        HttpClient client, string parent, string child, LineageType lineageType, decimal qty)
    {
        await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = parent, childLotNumber = child, lineageType,
            workOrderID = (int?)null, routingStepID = (int?)null,
            quantityConsumed = qty, uom = "KG"
        });
    }
}
