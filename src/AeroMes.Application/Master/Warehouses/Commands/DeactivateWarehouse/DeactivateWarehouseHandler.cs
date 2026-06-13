using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.DeactivateWarehouse;

public class DeactivateWarehouseHandler(
    IWarehouseRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeactivateWarehouseCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeactivateWarehouseCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.WarehouseId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Kho '{cmd.WarehouseId}' không tồn tại hoặc đã bị xóa.");

        entity.Deactivate(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
