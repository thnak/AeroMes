using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetShipmentList;

public record GetShipmentListQuery(ShipmentStatus? Status) : IQuery<IReadOnlyList<ShipmentSummaryDto>>;

public record ShipmentSummaryDto(
    int ShipmentId,
    string ShipmentCode,
    int? SoId,
    string CustomerName,
    DateOnly RequestedShipDate,
    ShipmentStatus Status,
    string? CarrierName,
    string? TrackingNumber,
    int LineCount,
    DateTime CreatedAt);
