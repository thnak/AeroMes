using MediatR;

namespace AeroMes.Application.Master.Operations.Commands.DeleteOperation;

public record DeleteOperationCommand(string Code) : IRequest;
