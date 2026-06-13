using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRouting;

public record DeleteRoutingCommand(int Id, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
