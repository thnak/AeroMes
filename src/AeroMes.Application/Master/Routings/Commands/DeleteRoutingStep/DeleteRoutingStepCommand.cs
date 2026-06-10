using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;

public record DeleteRoutingStepCommand(int RoutingStepId) : ICommand;
