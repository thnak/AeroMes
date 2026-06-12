using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.DeleteProductionTeam;

public record DeleteProductionTeamCommand(string Code, string? DeletedBy) : ICommand;
