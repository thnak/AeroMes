using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.DuplicateProductionTeam;

public class DuplicateProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<DuplicateProductionTeamCommand, string>
{
    public async Task<string> HandleAsync(DuplicateProductionTeamCommand cmd, CancellationToken ct)
    {
        var source = await repo.GetByCodeAsync(cmd.SourceCode, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionTeam), cmd.SourceCode);

        var copy = source.Duplicate(cmd.NewCode, cmd.CreatedBy);
        await repo.AddAsync(copy, ct);
        await uow.SaveChangesAsync(ct);
        return copy.TeamCode;
    }
}
