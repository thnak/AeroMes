using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetShipmentById;

public record GetShipmentByIdQuery(int ShipmentId) : IQuery<ShipmentDetailDto?>;

public record ShipmentDetailDto(
    int ShipmentId,
    string ShipmentCode,
    int? SoId,
    string CustomerName,
    DateOnly RequestedShipDate,
    ShipmentStatus Status,
    string? CarrierName,
    string? TrackingNumber,
    DateTime CreatedAt,
    IReadOnlyList<ShipmentLineDto> Lines);

public record ShipmentLineDto(
    int LineId,
    string ProductCode,
    decimal OrderedQty,
    decimal PickedQty,
    decimal PackedQty);
