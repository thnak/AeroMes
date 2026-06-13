using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetPickListForShipment;

public record GetPickListForShipmentQuery(int ShipmentId) : IQuery<PickListDetailDto?>;

public record PickListDetailDto(
    int PickListId,
    int ShipmentId,
    string? AssignedTo,
    PickListStatus Status,
    DateTime? CompletedAt,
    IReadOnlyList<PickListLineDto> Lines);

public record PickListLineDto(
    long PickLineId,
    int ShipmentLineId,
    string ProductCode,
    string LotNumber,
    int LocationId,
    int? BinId,
    decimal RequiredQty,
    decimal PickedQty,
    bool IsConfirmed,
    int PickSequence);
