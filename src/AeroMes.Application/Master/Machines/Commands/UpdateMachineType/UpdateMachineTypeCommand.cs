using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineType;

public sealed record UpdateMachineTypeCommand(string MachineCode, string MachineType, string UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
