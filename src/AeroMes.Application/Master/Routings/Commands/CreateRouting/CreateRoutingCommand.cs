using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.CreateRouting;

public record CreateRoutingCommand(
    string Code,
    string Name,
    string ProductCode,
    bool IsDefault,
    string? CreatedBy) : IRequest<int>;
