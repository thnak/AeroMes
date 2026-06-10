using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Operations.Commands.UpdateOperation;

public record UpdateOperationCommand(
    string Code,
    string Name,
    string? Description) : ICommand;
