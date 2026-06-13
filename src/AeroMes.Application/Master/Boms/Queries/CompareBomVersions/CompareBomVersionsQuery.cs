using AeroMes.Application.Master.Boms.Queries.GetActiveBom;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Boms.Queries.CompareBomVersions;

public record CompareBomVersionsQuery(
    string ProductCode,
    string FromVersion,
    string ToVersion) : IQuery<QueryResult<BomCompareDto>>;

public record BomCompareDto(
    string ProductCode,
    string FromVersion,
    string ToVersion,
    IReadOnlyList<BomLineDto> Added,
    IReadOnlyList<BomLineDto> Removed,
    IReadOnlyList<BomLineChangeDto> Changed);

public record BomLineChangeDto(
    string ComponentCode,
    string? ComponentName,
    decimal OldRequiredQty,
    decimal NewRequiredQty,
    string OldUoMCode,
    string NewUoMCode,
    decimal OldScrapFactor,
    decimal NewScrapFactor);
