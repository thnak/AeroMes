using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRouting;

public record DeleteRoutingCommand(int Id) : IRequest;
