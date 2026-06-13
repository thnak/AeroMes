using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetGrnList;

public record GetGrnListQuery(GrnStatus? Status, int? PoId)
    : IQuery<IReadOnlyList<GrnListDto>>;

public record GrnListDto(
    int GrnId,
    string GrnCode,
    int? PoId,
    int StorageLocationId,
    string ReceivedBy,
    DateTime ReceivedAt,
    string Status,
    string? DeliveryNoteRef,
    int LineCount,
    DateTime CreatedAt);
