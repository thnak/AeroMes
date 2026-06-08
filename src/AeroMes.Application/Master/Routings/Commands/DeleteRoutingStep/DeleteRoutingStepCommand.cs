using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;

public record DeleteRoutingStepCommand(int RoutingStepId) : IRequest;
