using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class LotTraceabilityRepository(AppDbContext db) : ILotTraceabilityRepository
{
    public Task AddLineageAsync(LotLineage lineage, CancellationToken ct)
    {
        db.LotLineages.Add(lineage);
        return Task.CompletedTask;
    }

    public Task AddEventAsync(LotEvent lotEvent, CancellationToken ct)
    {
        db.LotEvents.Add(lotEvent);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<LotGenealogyDto> BackwardTraceAsync(string lotNumber, int maxDepth, CancellationToken ct)
    {
        var edges = await RunBackwardCteAsync(lotNumber.ToUpperInvariant(), maxDepth, ct);
        return BuildGenealogyDto(lotNumber.ToUpperInvariant(), "Backward", edges);
    }

    public async Task<LotGenealogyDto> ForwardTraceAsync(string lotNumber, int maxDepth, CancellationToken ct)
    {
        var edges = await RunForwardCteAsync(lotNumber.ToUpperInvariant(), maxDepth, ct);
        return BuildGenealogyDto(lotNumber.ToUpperInvariant(), "Forward", edges);
    }

    public async Task<LotGenealogyDto> BidirectionalTraceAsync(string lotNumber, int maxDepth, CancellationToken ct)
    {
        var upperLot = lotNumber.ToUpperInvariant();
        var backward = await RunBackwardCteAsync(upperLot, maxDepth, ct);
        var forward = await RunForwardCteAsync(upperLot, maxDepth, ct);

        var allEdges = backward.Concat(forward).DistinctBy(e => (e.ParentLotNumber, e.ChildLotNumber)).ToList();
        return BuildGenealogyDto(upperLot, "Bidirectional", allEdges);
    }

    public async Task<IReadOnlyList<LotEventDto>> GetEventTimelineAsync(
        string lotNumber, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var upperLot = lotNumber.ToUpperInvariant();
        var q = db.LotEvents.AsNoTracking()
            .Where(e => e.LotNumber == upperLot);
        if (from.HasValue) q = q.Where(e => e.EventTimestamp >= from.Value);
        if (to.HasValue) q = q.Where(e => e.EventTimestamp <= to.Value);

        var events = await q.OrderBy(e => e.EventTimestamp).ToListAsync(ct);
        return events.Select(e => new LotEventDto(
            e.EventId, e.EventType.ToString(), e.LotNumber, e.ProductCode,
            e.WorkOrderID, e.RoutingStepID, e.LocationID,
            e.Quantity, e.UoM, e.Payload, e.OperatorCode, e.EquipmentCode,
            e.EventTimestamp, e.RecordedAt, e.SourceSystem.ToString())).ToList();
    }

    // ── CTE helpers ──────────────────────────────────────────────────────────

    private async Task<List<LineageEdgeDto>> RunBackwardCteAsync(
        string rootLot, int maxDepth, CancellationToken ct)
    {
        const string sql = """
            WITH BackwardTrace AS (
                SELECT ParentLotNumber, ChildLotNumber, WorkOrderID, RoutingStepID,
                       QuantityConsumed, LineageType, UoM, 1 AS Depth
                FROM trace.LotLineages
                WHERE ChildLotNumber = @rootLot
                UNION ALL
                SELECT ll.ParentLotNumber, ll.ChildLotNumber, ll.WorkOrderID, ll.RoutingStepID,
                       ll.QuantityConsumed, ll.LineageType, ll.UoM, bt.Depth + 1
                FROM trace.LotLineages ll
                INNER JOIN BackwardTrace bt ON ll.ChildLotNumber = bt.ParentLotNumber
                WHERE bt.Depth < @maxDepth
            )
            SELECT DISTINCT ParentLotNumber, ChildLotNumber, WorkOrderID, RoutingStepID,
                            QuantityConsumed, LineageType, UoM
            FROM BackwardTrace
            OPTION (MAXRECURSION 50)
            """;

        return await ExecuteEdgeQueryAsync(sql, rootLot, maxDepth, ct);
    }

    private async Task<List<LineageEdgeDto>> RunForwardCteAsync(
        string rootLot, int maxDepth, CancellationToken ct)
    {
        const string sql = """
            WITH ForwardTrace AS (
                SELECT ParentLotNumber, ChildLotNumber, WorkOrderID, RoutingStepID,
                       QuantityConsumed, LineageType, UoM, 1 AS Depth
                FROM trace.LotLineages
                WHERE ParentLotNumber = @rootLot
                UNION ALL
                SELECT ll.ParentLotNumber, ll.ChildLotNumber, ll.WorkOrderID, ll.RoutingStepID,
                       ll.QuantityConsumed, ll.LineageType, ll.UoM, ft.Depth + 1
                FROM trace.LotLineages ll
                INNER JOIN ForwardTrace ft ON ll.ParentLotNumber = ft.ChildLotNumber
                WHERE ft.Depth < @maxDepth
            )
            SELECT DISTINCT ParentLotNumber, ChildLotNumber, WorkOrderID, RoutingStepID,
                            QuantityConsumed, LineageType, UoM
            FROM ForwardTrace
            OPTION (MAXRECURSION 50)
            """;

        return await ExecuteEdgeQueryAsync(sql, rootLot, maxDepth, ct);
    }

    private async Task<List<LineageEdgeDto>> ExecuteEdgeQueryAsync(
        string sql, string rootLot, int maxDepth, CancellationToken ct)
    {
        var results = new List<LineageEdgeDto>();
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync(ct);
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SqlParameter("@rootLot", rootLot));
            cmd.Parameters.Add(new SqlParameter("@maxDepth", maxDepth));

            // Columns: 0=ParentLotNumber, 1=ChildLotNumber, 2=WorkOrderID,
            //          3=RoutingStepID, 4=QuantityConsumed, 5=LineageType, 6=UoM
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                results.Add(new LineageEdgeDto(
                    reader.GetString(0),                                   // ParentLotNumber
                    reader.GetString(1),                                   // ChildLotNumber
                    reader.GetString(5),                                   // LineageType (NOT NULL)
                    reader.IsDBNull(4) ? null : reader.GetDecimal(4),    // QuantityConsumed
                    reader.IsDBNull(6) ? null : reader.GetString(6),     // UoM
                    reader.IsDBNull(2) ? null : reader.GetInt32(2),      // WorkOrderID
                    reader.IsDBNull(3) ? null : reader.GetInt32(3)));    // RoutingStepID
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
        return results;
    }

    private static LotGenealogyDto BuildGenealogyDto(
        string rootLot, string direction, List<LineageEdgeDto> edges)
    {
        var nodeSet = new Dictionary<string, LineageNodeDto>();

        void AddNode(string lot, int depth)
        {
            if (!nodeSet.ContainsKey(lot))
                nodeSet[lot] = new LineageNodeDto(lot, null, null, depth);
        }

        AddNode(rootLot, 0);
        foreach (var edge in edges)
        {
            AddNode(edge.ParentLotNumber, 0);
            AddNode(edge.ChildLotNumber, 0);
        }

        return new LotGenealogyDto(rootLot, direction, [.. nodeSet.Values], edges);
    }
}
