using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteAisle;

public class DeleteAisleHandler(IAisleRepository repo, IUnitOfWork uow)
    : ICommandHandler<DeleteAisleCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteAisleCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.AisleId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Lối đi '{cmd.AisleId}' không tồn tại.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
