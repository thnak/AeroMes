using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.SetDisassemblyBomStatus;

public record SetDisassemblyBomStatusCommand(
    int DisassemblyBomId,
    bool IsActive,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
