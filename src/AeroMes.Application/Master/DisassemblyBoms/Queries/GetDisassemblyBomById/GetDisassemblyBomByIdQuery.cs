using AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBoms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBomById;

public record GetDisassemblyBomByIdQuery(int DisassemblyBomId) : IQuery<DisassemblyBomDetailDto?>;

public record DisassemblyBomLineDto(
    int LineId,
    int LineNo,
    string ComponentCode,
    string? ComponentName,
    string ComponentType,
    decimal? RecoveryRate,
    decimal? FixedQuantity,
    string UoMCode,
    string? Notes);

public record DisassemblyBomDetailDto(
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
    DateTime CreatedAt,
    IReadOnlyList<DisassemblyBomLineDto> Lines);
