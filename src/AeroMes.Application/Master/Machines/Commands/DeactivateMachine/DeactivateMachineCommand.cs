using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.DeactivateMachine;

public record DeactivateMachineCommand(string Code, string UpdatedBy) : ICommand<ValidationResult<Unit>>;
