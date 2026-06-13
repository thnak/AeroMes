using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteRack;

public class DeleteRackHandler(IRackRepository repo, IUnitOfWork uow)
    : ICommandHandler<DeleteRackCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteRackCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.RackId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Kệ '{cmd.RackId}' không tồn tại.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
