using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteZone;

public class DeleteZoneHandler(IWarehouseZoneRepository repo, IUnitOfWork uow)
    : ICommandHandler<DeleteZoneCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteZoneCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.ZoneId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Khu vực '{cmd.ZoneId}' không tồn tại.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
