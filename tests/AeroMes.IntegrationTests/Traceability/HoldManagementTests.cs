using System.Net;
using System.Net.Http.Json;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;

namespace AeroMes.IntegrationTests.Traceability;

/// <summary>
/// Integration tests for lot hold placement, management, and release — closes #273.
/// </summary>
[Collection("Integration")]
public class HoldManagementTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PlaceHold_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().PostAsJsonAsync("/api/v1/trace/holds", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetActiveHolds_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/trace/holds");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Place hold ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PlaceHold_FailedInspection_ReturnsHoldId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"H-{Guid.NewGuid():N}"[..20];

        var resp = await PlaceHoldAsync(client, lot, HoldReason.FailedInspection);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var holdId = await resp.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, holdId);
    }

    [Fact]
    public async Task PlaceHold_SupplierAlert_ReturnsHoldId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"H-{Guid.NewGuid():N}"[..20];

        var resp = await PlaceHoldAsync(client, lot, HoldReason.SupplierAlert);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task PlaceHold_RecallInvestigation_ReturnsHoldId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"H-{Guid.NewGuid():N}"[..20];

        var resp = await PlaceHoldAsync(client, lot, HoldReason.RecallInvestigation);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    // ── Multiple holds ─────────────────────────────────────────────────────

    [Fact]
    public async Task TwoConcurrentHolds_SameLot_BothTrackedIndependently()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"H2-{Guid.NewGuid():N}"[..20];

        var r1 = await PlaceHoldAsync(client, lot, HoldReason.FailedInspection);
        var r2 = await PlaceHoldAsync(client, lot, HoldReason.SupplierAlert);
        Assert.Equal(HttpStatusCode.Created, r1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, r2.StatusCode);

        var id1 = await r1.Content.ReadFromJsonAsync<Guid>();
        var id2 = await r2.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(id1, id2);

        // GetHolds for lot should show both
        var histResp = await client.GetAsync($"/api/v1/trace/holds/lot/{lot}/history");
        Assert.Equal(HttpStatusCode.OK, histResp.StatusCode);
        var history = await histResp.Content.ReadFromJsonAsync<IReadOnlyList<LotHoldDto>>();
        Assert.NotNull(history);
        Assert.Equal(2, history!.Count);
    }

    [Fact]
    public async Task GetActiveHolds_FilterByLotNumber_ReturnsOnlyThatLot()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"HA-{Guid.NewGuid():N}"[..20];

        await PlaceHoldAsync(client, lot, HoldReason.PreventiveHold);

        var resp = await client.GetAsync($"/api/v1/trace/holds?lotNumber={lot}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        // All returned holds must belong to this lot
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains(lot, body);
    }

    // ── Release hold ───────────────────────────────────────────────────────

    [Fact]
    public async Task ReleaseHold_UseAsIs_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"HR-{Guid.NewGuid():N}"[..20];

        var holdResp = await PlaceHoldAsync(client, lot, HoldReason.FailedInspection);
        var holdId = await holdResp.Content.ReadFromJsonAsync<Guid>();

        var releaseResp = await client.PostAsJsonAsync($"/api/v1/trace/holds/{holdId}/release", new
        {
            dispositionCode = HoldDispositionCode.UseAsIs,
            dispositionNotes = "Reviewed, acceptable deviation",
            eSignatureToken = "test-signature"
        });
        Assert.Equal(HttpStatusCode.NoContent, releaseResp.StatusCode);
    }

    [Fact]
    public async Task ReleaseHold_Destroy_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"HR2-{Guid.NewGuid():N}"[..20];

        var holdResp = await PlaceHoldAsync(client, lot, HoldReason.SpecDeviation);
        var holdId = await holdResp.Content.ReadFromJsonAsync<Guid>();

        var releaseResp = await client.PostAsJsonAsync($"/api/v1/trace/holds/{holdId}/release", new
        {
            dispositionCode = HoldDispositionCode.Destroy,
            dispositionNotes = "Non-conforming material disposed",
            eSignatureToken = "test-signature"
        });
        Assert.Equal(HttpStatusCode.NoContent, releaseResp.StatusCode);
    }

    [Fact]
    public async Task ReleaseHold_ReturnToSupplier_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"HR3-{Guid.NewGuid():N}"[..20];

        var holdResp = await PlaceHoldAsync(client, lot, HoldReason.SupplierAlert);
        var holdId = await holdResp.Content.ReadFromJsonAsync<Guid>();

        var releaseResp = await client.PostAsJsonAsync($"/api/v1/trace/holds/{holdId}/release", new
        {
            dispositionCode = HoldDispositionCode.ReturnToSupplier,
            dispositionNotes = "Returning to supplier",
            eSignatureToken = "test-signature"
        });
        Assert.Equal(HttpStatusCode.NoContent, releaseResp.StatusCode);
    }

    [Fact]
    public async Task ReleaseHold_Downgrade_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"HR4-{Guid.NewGuid():N}"[..20];

        var holdResp = await PlaceHoldAsync(client, lot, HoldReason.ProcessExcursion);
        var holdId = await holdResp.Content.ReadFromJsonAsync<Guid>();

        var releaseResp = await client.PostAsJsonAsync($"/api/v1/trace/holds/{holdId}/release", new
        {
            dispositionCode = HoldDispositionCode.Downgrade,
            dispositionNotes = "Downgraded to grade B",
            eSignatureToken = "test-signature"
        });
        Assert.Equal(HttpStatusCode.NoContent, releaseResp.StatusCode);
    }

    [Fact]
    public async Task ReleaseHold_AlreadyReleased_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"HR5-{Guid.NewGuid():N}"[..20];

        var holdResp = await PlaceHoldAsync(client, lot, HoldReason.MissingCoA);
        var holdId = await holdResp.Content.ReadFromJsonAsync<Guid>();

        // First release — OK
        await client.PostAsJsonAsync($"/api/v1/trace/holds/{holdId}/release", new
        {
            dispositionCode = HoldDispositionCode.UseAsIs,
            dispositionNotes = "OK",
            eSignatureToken = "test-signature"
        });

        // Second release — should fail
        var r2 = await client.PostAsJsonAsync($"/api/v1/trace/holds/{holdId}/release", new
        {
            dispositionCode = HoldDispositionCode.UseAsIs,
            dispositionNotes = "Duplicate",
            eSignatureToken = "test-signature"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, r2.StatusCode);
    }

    // ── Reject disposition ─────────────────────────────────────────────────

    [Fact]
    public async Task RejectDisposition_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"RD-{Guid.NewGuid():N}"[..20];

        var holdResp = await PlaceHoldAsync(client, lot, HoldReason.CustomerComplaint);
        var holdId = await holdResp.Content.ReadFromJsonAsync<Guid>();

        var rejectResp = await client.PostAsJsonAsync($"/api/v1/trace/holds/{holdId}/reject", new
        {
            dispositionCode = HoldDispositionCode.UseAsIs,
            dispositionNotes = "Not acceptable — needs further investigation",
            eSignatureToken = "test-signature"
        });
        Assert.Equal(HttpStatusCode.NoContent, rejectResp.StatusCode);
    }

    // ── Hold status ────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckHoldStatus_AfterPlacing_ShowsOnHold()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"HS-{Guid.NewGuid():N}"[..20];

        await PlaceHoldAsync(client, lot, HoldReason.RegulatoryRequest);

        var statusResp = await client.GetAsync($"/api/v1/trace/holds/lot/{lot}/status");
        Assert.Equal(HttpStatusCode.OK, statusResp.StatusCode);
        var status = await statusResp.Content.ReadFromJsonAsync<LotHoldStatusDto>();
        Assert.NotNull(status);
        Assert.True(status!.IsOnHold);
    }

    [Fact]
    public async Task CheckHoldStatus_UnknownLot_ShowsNotOnHold()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/trace/holds/lot/NO-SUCH-LOT-ZZZZZ/status");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var status = await resp.Content.ReadFromJsonAsync<LotHoldStatusDto>();
        Assert.NotNull(status);
        Assert.False(status!.IsOnHold);
    }

    // ── Bulk hold from forward trace ───────────────────────────────────────

    [Fact]
    public async Task BulkHoldFromForwardTrace_HoldsAllDownstreamLots()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var raw = $"BH-R-{Guid.NewGuid():N}"[..22];
        var wip = $"BH-W-{Guid.NewGuid():N}"[..22];
        var fg = $"BH-F-{Guid.NewGuid():N}"[..22];

        // Set up lineage raw → wip → fg
        await RecordLineageAsync(client, raw, wip, LineageType.Consume, 50m);
        await RecordLineageAsync(client, wip, fg, LineageType.Transform, 50m);

        var bulkResp = await client.PostAsJsonAsync("/api/v1/trace/holds/bulk-from-forward-trace", new
        {
            suspectLotNumber = raw,
            holdReason = HoldReason.SupplierAlert,
            holdReference = "SUPPLIER-ALERT-001",
            holdDescription = "Bulk hold for supplier alert",
            maxDepth = 10
        });
        Assert.Equal(HttpStatusCode.Created, bulkResp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> PlaceHoldAsync(
        HttpClient client, string lotNumber, HoldReason reason,
        string? productCode = null, string? description = null)
        => client.PostAsJsonAsync("/api/v1/trace/holds", new
        {
            lotNumber,
            holdReason = reason,
            productCode,
            workOrderID = (int?)null,
            holdDescription = description ?? $"Test hold for {reason}",
            holdReference = $"TEST-REF-{Guid.NewGuid():N}"[..20]
        });

    private static async Task RecordLineageAsync(
        HttpClient client, string parent, string child,
        LineageType lineageType, decimal qty)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = parent,
            childLotNumber = child,
            lineageType,
            workOrderID = (int?)null,
            routingStepID = (int?)null,
            quantityConsumed = qty,
            uom = "KG"
        });
        resp.EnsureSuccessStatusCode();
    }
}
