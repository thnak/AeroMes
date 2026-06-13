using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;

public record DeleteRoutingStepCommand(int RoutingStepId) : ICommand<ValidationResult<Unit>>;
