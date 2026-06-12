using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.RemoveTeamMember;

public class RemoveTeamMemberHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<RemoveTeamMemberCommand>
{
    public async Task HandleAsync(RemoveTeamMemberCommand cmd, CancellationToken ct)
    {
        var team = await repo.GetByCodeAsync(cmd.TeamCode, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionTeam), cmd.TeamCode);

        team.RemoveMember(cmd.EmployeeCode, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
