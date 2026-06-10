using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRouting;

public record DeleteRoutingCommand(int Id, string? DeletedBy = null) : ICommand;
