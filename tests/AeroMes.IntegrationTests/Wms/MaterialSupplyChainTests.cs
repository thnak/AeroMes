using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Wms.Commands.CreateMaterialSupplyRequest;
using AeroMes.Domain.Wms;

namespace AeroMes.IntegrationTests.Wms;

/// <summary>
/// Integration tests for Material Supply Chain (Request → Approve/Reject) — closes #269.
/// </summary>
[Collection("Integration")]
public class MaterialSupplyChainTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRequests_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/material-supply-requests");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create supply request ──────────────────────────────────────────────

    [Fact]
    public async Task CreateMaterialSupplyRequest_ValidRequest_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateRequestAsync(client);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<MaterialSupplyRequestCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.RequestId > 0);
    }

    [Fact]
    public async Task CreateMaterialSupplyRequest_NoLines_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/material-supply-requests", new
        {
            requestType = MaterialSupplyRequestType.MaterialIssuance,
            requesterUnit = "PROD-LINE-1",
            requiredByDate = (DateTime?)DateTime.UtcNow.AddDays(3),
            notes = (string?)null,
            lines = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    // ── Submit ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitRequest_DraftRequest_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateRequestAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<MaterialSupplyRequestCreatedResult>();

        var submitResp = await client.PostAsync($"/api/v1/material-supply-requests/{result!.RequestId}/submit", null);
        Assert.Equal(HttpStatusCode.NoContent, submitResp.StatusCode);
    }

    // ── Approve ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveRequest_SubmittedRequest_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var requestId = await CreateAndSubmitRequestAsync(client);

        var approveResp = await client.PostAsync($"/api/v1/material-supply-requests/{requestId}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approveResp.StatusCode);
    }

    // ── Reject ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RejectRequest_SubmittedRequest_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var requestId = await CreateAndSubmitRequestAsync(client);

        var rejectResp = await client.PostAsJsonAsync($"/api/v1/material-supply-requests/{requestId}/reject",
            new { reason = "Insufficient justification" });
        Assert.Equal(HttpStatusCode.NoContent, rejectResp.StatusCode);
    }

    // ── Query ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRequests_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/material-supply-requests");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetRequestById_AfterCreate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateRequestAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<MaterialSupplyRequestCreatedResult>();

        var resp = await client.GetAsync($"/api/v1/material-supply-requests/{result!.RequestId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetRequestById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/material-supply-requests/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── MaterialTransferSlip ───────────────────────────────────────────────

    [Fact]
    public async Task GetTransferSlips_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/material-transfer-slips");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetTransferSlips_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/material-transfer-slips");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreateRequestAsync(HttpClient client)
        => client.PostAsJsonAsync("/api/v1/material-supply-requests", new
        {
            requestType = MaterialSupplyRequestType.MaterialIssuance,
            requesterUnit = "PROD-LINE-1",
            requiredByDate = (DateTime?)DateTime.UtcNow.AddDays(3),
            notes = "Integration test",
            lines = new[]
            {
                new { productCode = "MAT-001", unitOfMeasure = "KG", requestedQuantity = 10m, warehouseId = (int?)null }
            }
        });

    private async Task<int> CreateAndSubmitRequestAsync(HttpClient client)
    {
        var createResp = await CreateRequestAsync(client);
        var result = await createResp.Content.ReadFromJsonAsync<MaterialSupplyRequestCreatedResult>();
        await client.PostAsync($"/api/v1/material-supply-requests/{result!.RequestId}/submit", null);
        return result.RequestId;
    }
}
