using AeroMes.Domain.Integration;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetMultiProductionOrders;

public sealed record GetMultiProductionOrdersQuery(
    MultiProductionOrderType? OrderType,
    MultiProductionOrderStatus? Status,
    DateTime? From,
    DateTime? To) : IQuery<IReadOnlyList<MultiProductionOrderSummaryDto>>;

public sealed record MultiProductionOrderSummaryDto(
    int MPOId,
    string OrderNumber,
    string OrderType,
    string? SourceReference,
    DateTime? PlannedStart,
    DateTime? PlannedEnd,
    string Status,
    byte Priority,
    string? ProductionUnit,
    int LineCount,
    DateTime? CreatedAt,
    string? CreatedBy);
