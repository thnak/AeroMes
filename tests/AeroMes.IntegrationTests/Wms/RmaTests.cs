using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Wms.Commands.AddRmaLine;
using AeroMes.Application.Wms.Commands.CreateRma;
using AeroMes.Application.Wms.Commands.ReceiveRma;
using AeroMes.Domain.Master;
using AeroMes.Domain.Wms;

namespace AeroMes.IntegrationTests.Wms;

/// <summary>
/// Integration tests for the RMA (Return Material Authorization) lifecycle — closes #267.
/// </summary>
[Collection("Integration")]
public class RmaTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRmas_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/rma");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create RMA ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRma_SupplierReturn_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateRmaAsync(client, ReturnDirection.SupplierReturn);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<RmaCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.RmaId > 0);
    }

    [Fact]
    public async Task CreateRma_CustomerReturn_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateRmaAsync(client, ReturnDirection.CustomerReturn);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    // ── Add line ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AddRmaLine_ValidLine_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var rmaResp = await CreateRmaAsync(client, ReturnDirection.SupplierReturn);
        var rma = await rmaResp.Content.ReadFromJsonAsync<RmaCreatedResult>();

        var lineResp = await client.PostAsJsonAsync($"/api/v1/rma/{rma!.RmaId}/lines", new
        {
            productCode = "MAT-001",
            lotNumber = $"RL-{Guid.NewGuid():N}"[..15],
            returnQty = 5m
        });
        Assert.Equal(HttpStatusCode.Created, lineResp.StatusCode);
        var lineResult = await lineResp.Content.ReadFromJsonAsync<RmaLineAddedResult>();
        Assert.NotNull(lineResult);
        Assert.True(lineResult!.RmaLineId > 0);
    }

    [Fact]
    public async Task AddRmaLine_ZeroQty_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var rmaResp = await CreateRmaAsync(client, ReturnDirection.SupplierReturn);
        var rma = await rmaResp.Content.ReadFromJsonAsync<RmaCreatedResult>();

        var lineResp = await client.PostAsJsonAsync($"/api/v1/rma/{rma!.RmaId}/lines", new
        {
            productCode = "MAT-001",
            lotNumber = $"RL-{Guid.NewGuid():N}"[..15],
            returnQty = 0m
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, lineResp.StatusCode);
    }

    // ── Authorize ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AuthorizeRma_WithLine_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var rmaId = await CreateRmaWithLineAsync(client, 10m);

        var authResp = await client.PostAsync($"/api/v1/rma/{rmaId}/authorize", null);
        Assert.Equal(HttpStatusCode.NoContent, authResp.StatusCode);
    }

    [Fact]
    public async Task AuthorizeRma_NoLines_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var rmaResp = await CreateRmaAsync(client, ReturnDirection.SupplierReturn);
        var rma = await rmaResp.Content.ReadFromJsonAsync<RmaCreatedResult>();

        var authResp = await client.PostAsync($"/api/v1/rma/{rma!.RmaId}/authorize", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, authResp.StatusCode);
    }

    // ── GetAll ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRmas_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/rma");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetRmaById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/rma/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreateRmaAsync(HttpClient client, ReturnDirection direction)
        => client.PostAsJsonAsync("/api/v1/rma", new
        {
            returnDirection = direction,
            sourceDocumentType = (string?)null,
            sourceDocumentId = (int?)null,
            returnReason = "Defective goods"
        });

    private async Task<int> CreateRmaWithLineAsync(HttpClient client, decimal qty)
    {
        var rmaResp = await CreateRmaAsync(client, ReturnDirection.SupplierReturn);
        var rma = await rmaResp.Content.ReadFromJsonAsync<RmaCreatedResult>();
        await client.PostAsJsonAsync($"/api/v1/rma/{rma!.RmaId}/lines", new
        {
            productCode = "MAT-001",
            lotNumber = $"RL-{Guid.NewGuid():N}"[..15],
            returnQty = qty
        });
        return rma.RmaId;
    }
}
