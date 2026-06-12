using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.DuplicateProductionTeam;

public record DuplicateProductionTeamCommand(
    string SourceCode,
    string NewCode,
    string? CreatedBy) : ICommand<string>;
