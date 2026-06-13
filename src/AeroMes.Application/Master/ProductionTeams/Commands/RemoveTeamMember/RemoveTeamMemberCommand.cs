using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductionTeams.Commands.RemoveTeamMember;

public record RemoveTeamMemberCommand(
    string TeamCode,
    string EmployeeCode,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
