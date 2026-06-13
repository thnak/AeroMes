using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBoms;

public record GetDisassemblyBomsQuery(
    string? SourceProductCode,
    string? Status) : IQuery<IReadOnlyList<DisassemblyBomSummaryDto>>;

public record DisassemblyBomSummaryDto(
    int DisassemblyBomId,
    string BomCode,
    string BomName,
    string SourceProductCode,
    string? SourceProductName,
    string BomType,
    decimal LossRatio,
    bool IsDefault,
    string Status,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    DateTime CreatedAt);
