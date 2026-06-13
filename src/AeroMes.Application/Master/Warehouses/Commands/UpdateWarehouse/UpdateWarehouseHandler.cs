using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseHandler(
    IWarehouseRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateWarehouseCommand> validator) : ICommandHandler<UpdateWarehouseCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateWarehouseCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var entity = await repo.GetByIdAsync(cmd.WarehouseId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Kho '{cmd.WarehouseId}' không tồn tại hoặc đã bị xóa.");

        entity.UpdateDetails(cmd.Name, cmd.Address, cmd.WarehouseType, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
