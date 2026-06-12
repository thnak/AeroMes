using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RemoveToolOperation;

public record RemoveToolOperationCommand(
    string ToolCode,
    int MappingId,
    string? UpdatedBy) : ICommand;
