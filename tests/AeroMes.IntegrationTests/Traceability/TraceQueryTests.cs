using System.Net;
using System.Net.Http.Json;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;

namespace AeroMes.IntegrationTests.Traceability;

/// <summary>
/// Integration tests for forward/backward trace queries — closes #271.
/// </summary>
[Collection("Integration")]
public class TraceQueryTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ForwardTrace_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/traceability/forward?lotNumber=X");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task BackwardTrace_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/traceability/backward?lotNumber=X");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Forward trace ──────────────────────────────────────────────────────

    [Fact]
    public async Task ForwardTrace_UntracedLot_ReturnsEmptyResult()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/traceability/forward?lotNumber=NO-LINEAGE-LOT-ZZZZZ");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.Empty(dto!.Nodes);
        Assert.Empty(dto.Edges);
    }

    [Fact]
    public async Task ForwardTrace_SingleHop_ReturnsChildLot()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var raw = $"FT-RAW-{Guid.NewGuid():N}"[..22];
        var wip = $"FT-WIP-{Guid.NewGuid():N}"[..22];

        await RecordLineageAsync(client, raw, wip, LineageType.Consume, 50m);

        var resp = await client.GetAsync($"/api/v1/traceability/forward?lotNumber={raw}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.Contains(dto!.Nodes, n => n.LotNumber == wip);
    }

    [Fact]
    public async Task ForwardTrace_MultiHop_ReturnsAllDescendants()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var raw = $"FT2-R-{Guid.NewGuid():N}"[..22];
        var wip = $"FT2-W-{Guid.NewGuid():N}"[..22];
        var fg = $"FT2-F-{Guid.NewGuid():N}"[..22];

        await RecordLineageAsync(client, raw, wip, LineageType.Consume, 30m);
        await RecordLineageAsync(client, wip, fg, LineageType.Transform, 30m);

        var resp = await client.GetAsync($"/api/v1/traceability/forward?lotNumber={raw}&maxDepth=5");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.Contains(dto!.Nodes, n => n.LotNumber == wip);
        Assert.Contains(dto.Nodes, n => n.LotNumber == fg);
    }

    [Fact]
    public async Task ForwardTrace_StopsAtLotWithNoFurtherLineage()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var raw = $"FT3-R-{Guid.NewGuid():N}"[..22];
        var leaf = $"FT3-L-{Guid.NewGuid():N}"[..22];

        await RecordLineageAsync(client, raw, leaf, LineageType.Consume, 20m);

        // Forward from leaf should return no further descendants
        var resp = await client.GetAsync($"/api/v1/traceability/forward?lotNumber={leaf}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.DoesNotContain(dto!.Nodes, n => n.LotNumber == raw); // not upstream
    }

    // ── Backward trace ─────────────────────────────────────────────────────

    [Fact]
    public async Task BackwardTrace_UntracedLot_ReturnsEmptyResult()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/traceability/backward?lotNumber=NO-LINEAGE-LOT-ZZZZZ");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.Empty(dto!.Nodes);
    }

    [Fact]
    public async Task BackwardTrace_FinishedGoodsLot_ReturnsSourceInputLots()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var raw = $"BT-R-{Guid.NewGuid():N}"[..22];
        var wip = $"BT-W-{Guid.NewGuid():N}"[..22];
        var fg = $"BT-F-{Guid.NewGuid():N}"[..22];

        await RecordLineageAsync(client, raw, wip, LineageType.Consume, 100m);
        await RecordLineageAsync(client, wip, fg, LineageType.Transform, 100m);

        var resp = await client.GetAsync($"/api/v1/traceability/backward?lotNumber={fg}&maxDepth=10");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.Contains(dto!.Nodes, n => n.LotNumber == wip);
        Assert.Contains(dto.Nodes, n => n.LotNumber == raw);
    }

    [Fact]
    public async Task BackwardTrace_BlendedMaterial_ReturnsMultipleParents()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var a = $"BL2-A-{Guid.NewGuid():N}"[..22];
        var b = $"BL2-B-{Guid.NewGuid():N}"[..22];
        var output = $"BL2-O-{Guid.NewGuid():N}"[..22];

        await RecordLineageAsync(client, a, output, LineageType.Merge, 40m);
        await RecordLineageAsync(client, b, output, LineageType.Merge, 60m);

        var resp = await client.GetAsync($"/api/v1/traceability/backward?lotNumber={output}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.Contains(dto!.Nodes, n => n.LotNumber == a);
        Assert.Contains(dto.Nodes, n => n.LotNumber == b);
    }

    // ── Bidirectional ──────────────────────────────────────────────────────

    [Fact]
    public async Task BidirectionalTrace_MiddleLot_ReturnsBothDirections()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var upstream = $"BI-U-{Guid.NewGuid():N}"[..22];
        var mid = $"BI-M-{Guid.NewGuid():N}"[..22];
        var downstream = $"BI-D-{Guid.NewGuid():N}"[..22];

        await RecordLineageAsync(client, upstream, mid, LineageType.Consume, 50m);
        await RecordLineageAsync(client, mid, downstream, LineageType.Transform, 50m);

        var resp = await client.GetAsync($"/api/v1/traceability/bidirectional?lotNumber={mid}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var dto = await resp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(dto);
        Assert.Contains(dto!.Nodes, n => n.LotNumber == upstream);
        Assert.Contains(dto.Nodes, n => n.LotNumber == downstream);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

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
