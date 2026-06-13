using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachine;

public record UpdateMachineCommand(
    string Code,
    string Name,
    int WorkCenterId,
    string? Brand,
    string? Model,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
