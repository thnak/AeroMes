using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.ActivateMachine;

public record ActivateMachineCommand(string Code, string UpdatedBy) : ICommand<ValidationResult<Unit>>;
