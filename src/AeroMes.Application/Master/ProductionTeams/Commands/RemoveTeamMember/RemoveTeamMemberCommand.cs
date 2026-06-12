using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.RemoveTeamMember;

public record RemoveTeamMemberCommand(
    string TeamCode,
    string EmployeeCode,
    string? UpdatedBy) : ICommand;
