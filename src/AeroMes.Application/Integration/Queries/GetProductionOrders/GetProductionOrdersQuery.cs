using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetProductionOrders;

public record GetProductionOrdersQuery(
    int? SoId,
    string? PoCode,
    string? ProductCode,
    string? Status) : IQuery<IReadOnlyList<ProductionOrderDto>>;

public record ProductionOrderDto(
    int POID,
    string POCode,
    int? SOID,
    string ProductCode,
    int TargetQuantity,
    string Status,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    DateTime SyncedAt);
