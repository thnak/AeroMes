using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineAttributes;

public sealed record UpdateMachineAttributesCommand(string MachineCode, string? CustomAttributesJson, string UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
