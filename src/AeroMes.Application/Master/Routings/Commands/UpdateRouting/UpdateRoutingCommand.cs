using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.UpdateRouting;

public record UpdateRoutingCommand(
    int Id,
    string Name,
    bool IsDefault,
    string UpdatedBy) : IRequest;
