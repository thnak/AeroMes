using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.SetMoldCompatibility;

public record SetMoldCompatibilityCommand(
    string MoldCode,
    string MachineCode,
    bool IsCompatible,
    string? Notes
) : ICommand<ValidationResult<Unit>>;
