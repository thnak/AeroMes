using System.Net;
using System.Net.Http.Json;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;

namespace AeroMes.IntegrationTests.Traceability;

/// <summary>
/// Integration tests for lot lineage recording and lot event timeline — closes #272.
/// </summary>
[Collection("Integration")]
public class LotLineageTests(AeroMesWebFactory factory)
{
    private static readonly string ParentLot = $"RM-{Guid.NewGuid():N}"[..20];
    private static readonly string ChildLot = $"WIP-{Guid.NewGuid():N}"[..20];
    private static readonly string GrandChildLot = $"FG-{Guid.NewGuid():N}"[..20];
    private static readonly string BlendLotA = $"BL-A-{Guid.NewGuid():N}"[..20];
    private static readonly string BlendLotB = $"BL-B-{Guid.NewGuid():N}"[..20];
    private static readonly string BlendOutputLot = $"BL-OUT-{Guid.NewGuid():N}"[..20];

    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordLineage_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().PostAsJsonAsync("/api/v1/traceability/lineage", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task AppendEvent_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().PostAsJsonAsync("/api/v1/traceability/lot-events", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Record lineage ─────────────────────────────────────────────────────

    [Fact]
    public async Task RecordLineage_ParentToChild_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = ParentLot,
            childLotNumber = ChildLot,
            lineageType = LineageType.Consume,
            workOrderID = (int?)null,
            routingStepID = (int?)null,
            quantityConsumed = 10.0m,
            uom = "KG"
        });
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task RecordLineage_MultipleParentsMergedIntoOneChild_AllRecorded()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        // Blend lot A into output
        var r1 = await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = BlendLotA,
            childLotNumber = BlendOutputLot,
            lineageType = LineageType.Merge,
            workOrderID = (int?)null,
            routingStepID = (int?)null,
            quantityConsumed = 5.0m,
            uom = "KG"
        });
        Assert.Equal(HttpStatusCode.NoContent, r1.StatusCode);

        // Blend lot B into same output
        var r2 = await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = BlendLotB,
            childLotNumber = BlendOutputLot,
            lineageType = LineageType.Merge,
            workOrderID = (int?)null,
            routingStepID = (int?)null,
            quantityConsumed = 5.0m,
            uom = "KG"
        });
        Assert.Equal(HttpStatusCode.NoContent, r2.StatusCode);

        // Backward trace from blended output → should find both inputs
        var traceResp = await client.GetAsync($"/api/v1/traceability/backward?lotNumber={BlendOutputLot}");
        Assert.Equal(HttpStatusCode.OK, traceResp.StatusCode);
        var genealogy = await traceResp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(genealogy);
        Assert.Contains(genealogy!.Nodes, n => n.LotNumber == BlendLotA);
        Assert.Contains(genealogy.Nodes, n => n.LotNumber == BlendLotB);
    }

    [Fact]
    public async Task RecordLineage_SameParentSameChild_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"SELF-{Guid.NewGuid():N}"[..20];
        var resp = await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = lot,
            childLotNumber = lot,
            lineageType = LineageType.Consume,
            workOrderID = (int?)null,
            routingStepID = (int?)null,
            quantityConsumed = (decimal?)null,
            uom = (string?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task RecordLineage_ChildSplitIntoMultipleGrandchildren_ForwardTraceShowsBoth()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var splitParent = $"SP-{Guid.NewGuid():N}"[..20];
        var split1 = $"S1-{Guid.NewGuid():N}"[..20];
        var split2 = $"S2-{Guid.NewGuid():N}"[..20];

        await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = splitParent, childLotNumber = split1,
            lineageType = LineageType.Split, workOrderID = (int?)null, routingStepID = (int?)null,
            quantityConsumed = 3.0m, uom = "PCS"
        });
        await client.PostAsJsonAsync("/api/v1/traceability/lineage", new
        {
            parentLotNumber = splitParent, childLotNumber = split2,
            lineageType = LineageType.Split, workOrderID = (int?)null, routingStepID = (int?)null,
            quantityConsumed = 7.0m, uom = "PCS"
        });

        var traceResp = await client.GetAsync($"/api/v1/traceability/forward?lotNumber={splitParent}");
        Assert.Equal(HttpStatusCode.OK, traceResp.StatusCode);
        var genealogy = await traceResp.Content.ReadFromJsonAsync<LotGenealogyDto>();
        Assert.NotNull(genealogy);
        Assert.Contains(genealogy!.Nodes, n => n.LotNumber == split1);
        Assert.Contains(genealogy.Nodes, n => n.LotNumber == split2);
    }

    // ── Append events ───────────────────────────────────────────────────────

    [Fact]
    public async Task AppendEvent_Received_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/traceability/lot-events", new
        {
            eventType = LotEventType.Received,
            lotNumber = ParentLot,
            productCode = "RAW-001",
            operatorCode = "SYS",
            eventTimestamp = DateTime.UtcNow,
            locationId = (int?)null,
            quantity = 100.0m,
            uom = "KG",
            payload = (string?)null,
            equipmentCode = (string?)null,
            sourceSystem = LotEventSourceSystem.WMS
        });
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task GetLotEventTimeline_ReturnsSortedEvents()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var lot = $"TL-{Guid.NewGuid():N}"[..20];

        // Append two events out of order
        var t1 = DateTime.UtcNow.AddMinutes(-5);
        var t2 = DateTime.UtcNow;

        await client.PostAsJsonAsync("/api/v1/traceability/lot-events", new
        {
            eventType = LotEventType.Produced, lotNumber = lot, productCode = "P-001",
            operatorCode = "OP1", eventTimestamp = t2,
            locationId = (int?)null, quantity = 50m, uom = "PCS",
            payload = (string?)null, equipmentCode = (string?)null, sourceSystem = LotEventSourceSystem.MES
        });
        await client.PostAsJsonAsync("/api/v1/traceability/lot-events", new
        {
            eventType = LotEventType.Received, lotNumber = lot, productCode = "P-001",
            operatorCode = "OP1", eventTimestamp = t1,
            locationId = (int?)null, quantity = 50m, uom = "PCS",
            payload = (string?)null, equipmentCode = (string?)null, sourceSystem = LotEventSourceSystem.WMS
        });

        var timelineResp = await client.GetAsync($"/api/v1/traceability/lot-events/{lot}");
        Assert.Equal(HttpStatusCode.OK, timelineResp.StatusCode);

        var events = await timelineResp.Content.ReadFromJsonAsync<IReadOnlyList<LotEventDto>>();
        Assert.NotNull(events);
        Assert.True(events!.Count >= 2);
        // Events should be sorted ascending by EventTimestamp
        for (int i = 1; i < events.Count; i++)
            Assert.True(events[i].EventTimestamp >= events[i - 1].EventTimestamp);
    }

    [Fact]
    public async Task GetLotEventTimeline_UnknownLot_ReturnsEmptyList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/traceability/lot-events/NO-SUCH-LOT-ZZZZZ");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var events = await resp.Content.ReadFromJsonAsync<IReadOnlyList<LotEventDto>>();
        Assert.NotNull(events);
        Assert.Empty(events!);
    }
}
