using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Boms.Commands.ActivateBomVersion;

public class ActivateBomVersionHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow) : ICommandHandler<ActivateBomVersionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ActivateBomVersionCommand cmd, CancellationToken ct)
    {
        var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct);
        if (header is null) return ValidationResult<Unit>.NotFound($"Entity '{$"{cmd.ProductCode}/{cmd.Version}"}' was not found.");

        // Only one Active version per product — the previous one is superseded automatically.
        var currentActive = await repo.GetActiveByProductAsync(cmd.ProductCode, ct);
        if (currentActive is not null && currentActive.BomHeaderId != header.BomHeaderId)
            currentActive.Supersede(cmd.UpdatedBy);

        header.Activate(cmd.EffectiveFrom, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
