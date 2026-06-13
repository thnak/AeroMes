using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Machines.Commands.DeleteMachine;

public record DeleteMachineCommand(string Code, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
