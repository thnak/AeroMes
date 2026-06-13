using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.CreateDisassemblyBom;

public record DisassemblyBomLineInput(
    int LineNo,
    string ComponentCode,
    DisassemblyComponentType ComponentType,
    decimal? RecoveryRate,
    decimal? FixedQuantity,
    string UoMCode,
    string? Notes = null);

public record DisassemblyBomCreatedResult(int DisassemblyBomId, string BomCode);

public record CreateDisassemblyBomCommand(
    string BomName,
    string SourceProductCode,
    DisassemblyBomType BomType,
    decimal LossRatio,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    IReadOnlyList<DisassemblyBomLineInput> Lines,
    string? CreatedBy) : ICommand<ValidationResult<DisassemblyBomCreatedResult>>;
