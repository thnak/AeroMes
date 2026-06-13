namespace AeroMes.Domain.Traceability.Repositories;

public sealed record LineageNodeDto(
    string LotNumber,
    string? ProductCode,
    string? HoldStatus,   // null = no hold, "OnHold", "Recalled"
    int Depth);

public sealed record LineageEdgeDto(
    string ParentLotNumber,
    string ChildLotNumber,
    string LineageType,
    decimal? QuantityConsumed,
    string? UoM,
    int? WorkOrderID,
    int? RoutingStepID);

public sealed record LotGenealogyDto(
    string RootLotNumber,
    string Direction,
    IReadOnlyList<LineageNodeDto> Nodes,
    IReadOnlyList<LineageEdgeDto> Edges);

public sealed record LotEventDto(
    long EventId,
    string EventType,
    string LotNumber,
    string ProductCode,
    int? WorkOrderID,
    int? RoutingStepID,
    int? LocationID,
    decimal? Quantity,
    string? UoM,
    string? Payload,
    string OperatorCode,
    string? EquipmentCode,
    DateTime EventTimestamp,
    DateTime RecordedAt,
    string SourceSystem);

public interface ILotTraceabilityRepository
{
    Task AddLineageAsync(LotLineage lineage, CancellationToken ct = default);
    Task AddEventAsync(LotEvent lotEvent, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    // Recursive CTE-based trace queries
    Task<LotGenealogyDto> BackwardTraceAsync(string lotNumber, int maxDepth, CancellationToken ct = default);
    Task<LotGenealogyDto> ForwardTraceAsync(string lotNumber, int maxDepth, CancellationToken ct = default);
    Task<LotGenealogyDto> BidirectionalTraceAsync(string lotNumber, int maxDepth, CancellationToken ct = default);

    // Event timeline
    Task<IReadOnlyList<LotEventDto>> GetEventTimelineAsync(
        string lotNumber, DateTime? from, DateTime? to, CancellationToken ct = default);
}
