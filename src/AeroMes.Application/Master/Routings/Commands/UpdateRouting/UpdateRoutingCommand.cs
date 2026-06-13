using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.UpdateRouting;

public record UpdateRoutingCommand(
    int Id,
    string Name,
    bool IsDefault,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
