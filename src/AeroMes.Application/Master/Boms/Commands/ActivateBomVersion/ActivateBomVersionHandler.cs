using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.ActivateBomVersion;

public class ActivateBomVersionHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow) : ICommandHandler<ActivateBomVersionCommand>
{
    public async Task HandleAsync(ActivateBomVersionCommand cmd, CancellationToken ct)
    {
        var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct)
            ?? throw new EntityNotFoundException(nameof(BomHeader), $"{cmd.ProductCode}/{cmd.Version}");

        // Only one Active version per product — the previous one is superseded automatically.
        var currentActive = await repo.GetActiveByProductAsync(cmd.ProductCode, ct);
        if (currentActive is not null && currentActive.BomHeaderId != header.BomHeaderId)
            currentActive.Supersede(cmd.UpdatedBy);

        header.Activate(cmd.EffectiveFrom, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
