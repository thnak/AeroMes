using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Queries.GetEngChanges;

public record GetEngChangesQuery(
    EcStatus? Status = null,
    EcType? EcType = null,
    string? Search = null) : IQuery<IReadOnlyList<EngChangeDto>>;

public record EngChangeDto(
    int EcId,
    string EcNumber,
    string EcType,
    string Title,
    string Reason,
    string Status,
    string Priority,
    string? RequestedBy,
    DateTime RequestedAt,
    DateOnly? TargetDate,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? AffectedProducts,
    string? SourceEcrNumber,
    int? NewBomHeaderId);
