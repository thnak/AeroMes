using AeroMes.Application.Common;
using AeroMes.Application.Master.DisassemblyBoms.Commands.CreateDisassemblyBom;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.UpdateDisassemblyBom;

public record UpdateDisassemblyBomCommand(
    int DisassemblyBomId,
    string BomName,
    decimal LossRatio,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    IReadOnlyList<DisassemblyBomLineInput> Lines,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
