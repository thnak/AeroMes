using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetRmaById;

public record GetRmaByIdQuery(int RmaId) : IQuery<RmaDetailDto?>;

public record RmaDetailDto(
    int RmaId,
    string RmaCode,
    ReturnDirection ReturnDirection,
    string? SourceDocumentType,
    int? SourceDocumentId,
    string ReturnReason,
    RmaStatus Status,
    string? AuthorizedBy,
    DateTime? AuthorizedAt,
    string? CreatedBy,
    DateTime CreatedAt,
    IReadOnlyList<RmaLineDto> Lines);

public record RmaLineDto(
    int RmaLineId,
    string ProductCode,
    string LotNumber,
    decimal ReturnQty,
    decimal ReceivedQty,
    RmaDisposition? Disposition,
    int? NcrId,
    long? StockMovementId);
