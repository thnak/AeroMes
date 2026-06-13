using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Wms.Queries.GetGrnDetail;

public record GetGrnDetailQuery(int GrnId) : IQuery<QueryResult<GrnDetailDto>>;

public record GrnDetailDto(
    int GrnId,
    string GrnCode,
    int? PoId,
    int StorageLocationId,
    string ReceivedBy,
    DateTime ReceivedAt,
    string Status,
    string? DeliveryNoteRef,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<GrnLineDetailDto> Lines);

public record GrnLineDetailDto(
    int GrnLineId,
    int? PoLineId,
    string ProductCode,
    string LotNumber,
    decimal ReceivedQty,
    DateOnly? ManufacturedDate,
    DateOnly? ExpiryDate,
    decimal? GrossWeightKg,
    string QcStatus,
    int? DestinationBinId);
