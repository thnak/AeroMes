using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.DeleteWarehouse;

public class DeleteWarehouseHandler(
    IWarehouseRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWarehouseCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteWarehouseCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.WarehouseId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Kho '{cmd.WarehouseId}' không tồn tại hoặc đã bị xóa.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
