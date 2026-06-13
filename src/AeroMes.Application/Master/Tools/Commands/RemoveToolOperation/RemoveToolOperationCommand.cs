using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Tools.Commands.RemoveToolOperation;

public record RemoveToolOperationCommand(
    string ToolCode,
    int MappingId,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
