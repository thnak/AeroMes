using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductionTeams.Commands.RemoveTeamMember;

public class RemoveTeamMemberHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<RemoveTeamMemberCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveTeamMemberCommand cmd, CancellationToken ct)
    {
        var team = await repo.GetByCodeAsync(cmd.TeamCode, ct);
        if (team is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.TeamCode}' was not found.");

        team.RemoveMember(cmd.EmployeeCode, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
