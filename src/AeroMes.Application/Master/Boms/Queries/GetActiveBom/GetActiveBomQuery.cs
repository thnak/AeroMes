using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.GetActiveBom;

public record GetActiveBomQuery(string ProductCode) : IQuery<BomVersionDetailDto?>;

public record BomVersionDetailDto(
    int BomHeaderId,
    string ProductCode,
    string Version,
    string Status,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    decimal BaseQuantity,
    string? EcoReference,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? Notes,
    IReadOnlyList<BomLineDto> Lines);

public record BomLineDto(
    int BomLineId,
    int LineNo,
    string ComponentCode,
    string? ComponentName,
    decimal RequiredQty,
    string UoMCode,
    decimal ScrapFactor,
    bool IsPhantom,
    string? Notes);
