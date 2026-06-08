using MediatR;

namespace AeroMes.Application.Master.Operations.Commands.CreateOperation;

public record CreateOperationCommand(
    string Code,
    string Name,
    string? Description) : IRequest<string>;
