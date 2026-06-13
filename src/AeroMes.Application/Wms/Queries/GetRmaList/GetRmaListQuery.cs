using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetRmaList;

public record GetRmaListQuery(
    ReturnDirection? Direction,
    RmaStatus? Status) : IQuery<IReadOnlyList<RmaSummaryDto>>;

public record RmaSummaryDto(
    int RmaId,
    string RmaCode,
    ReturnDirection ReturnDirection,
    string? SourceDocumentType,
    int? SourceDocumentId,
    string ReturnReason,
    RmaStatus Status,
    string? AuthorizedBy,
    DateTime? AuthorizedAt,
    int LineCount,
    string? CreatedBy,
    DateTime CreatedAt);
