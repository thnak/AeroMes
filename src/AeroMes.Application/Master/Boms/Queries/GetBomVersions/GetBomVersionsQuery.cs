using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.GetBomVersions;

public record GetBomVersionsQuery(string ProductCode) : IQuery<IReadOnlyList<BomVersionDto>>;

public record BomVersionDto(
    int BomHeaderId,
    string Version,
    string Status,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    decimal BaseQuantity,
    string? EcoReference,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    int LineCount,
    DateTime CreatedAt);
