using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.DeleteDisassemblyBom;

public record DeleteDisassemblyBomCommand(int DisassemblyBomId, string? DeletedBy = null)
    : ICommand<ValidationResult<Unit>>;
