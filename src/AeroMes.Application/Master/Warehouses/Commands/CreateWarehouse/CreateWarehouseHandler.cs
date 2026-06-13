using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseHandler(
    IWarehouseRepository repo,
    IUnitOfWork uow,
    IValidator<CreateWarehouseCommand> validator) : ICommandHandler<CreateWarehouseCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateWarehouseCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        if (await repo.CodeExistsAsync(cmd.Code, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["Code"] = [$"Mã kho '{cmd.Code}' đã tồn tại."]
            });

        var entity = Warehouse.Create(
            cmd.Code,
            cmd.Name,
            cmd.WarehouseType,
            cmd.Address,
            cmd.IntegrationSource,
            cmd.CreatedBy);

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(entity.WarehouseId);
    }
}
