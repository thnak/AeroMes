using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ActivateBin;

public class ActivateBinHandler(IBinRepository repo, IUnitOfWork uow)
    : ICommandHandler<ActivateBinCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ActivateBinCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.BinId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Ô '{cmd.BinId}' không tồn tại.");

        entity.Activate(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
