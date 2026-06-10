using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Operations.Commands.DeleteOperation;

public record DeleteOperationCommand(string Code, string? DeletedBy = null) : ICommand;
