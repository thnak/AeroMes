using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.AddTeamMember;

public record AddTeamMemberCommand(
    string TeamCode,
    string EmployeeCode,
    bool IsLeader,
    string? UpdatedBy) : ICommand<ValidationResult<int>>;
