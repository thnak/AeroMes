using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.SetDisassemblyBomDefault;

public record SetDisassemblyBomDefaultCommand(
    int DisassemblyBomId,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
