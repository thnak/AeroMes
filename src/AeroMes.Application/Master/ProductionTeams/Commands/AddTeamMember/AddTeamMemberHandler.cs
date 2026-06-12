using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.AddTeamMember;

public class AddTeamMemberHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<AddTeamMemberCommand, int>
{
    public async Task<int> HandleAsync(AddTeamMemberCommand cmd, CancellationToken ct)
    {
        var team = await repo.GetByCodeAsync(cmd.TeamCode, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionTeam), cmd.TeamCode);

        var member = team.AddMember(cmd.EmployeeCode, cmd.IsLeader, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return member.MemberId;
    }
}
