using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteBin;

public class DeleteBinHandler(IBinRepository repo, IUnitOfWork uow)
    : ICommandHandler<DeleteBinCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteBinCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.BinId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Ô '{cmd.BinId}' không tồn tại.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
