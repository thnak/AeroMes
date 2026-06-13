using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateBeginningInventoryEntry;

public class CreateBeginningInventoryEntryHandler(
    IBeginningInventoryEntryRepository repo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateBeginningInventoryEntryCommand> validator)
    : ICommandHandler<CreateBeginningInventoryEntryCommand, ValidationResult<BeginningInventoryEntryCreatedResult>>
{
    public async Task<ValidationResult<BeginningInventoryEntryCreatedResult>> HandleAsync(
        CreateBeginningInventoryEntryCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<BeginningInventoryEntryCreatedResult>.Invalid(validation.ToErrorDictionary());

        var warehouse = await warehouseRepo.GetByIdAsync(cmd.WarehouseId, ct);
        if (warehouse is null)
            return ValidationResult<BeginningInventoryEntryCreatedResult>.NotFound(
                $"Kho '{cmd.WarehouseId}' không tồn tại.");

        var product = await productRepo.GetByCodeAsync(cmd.ProductCode, ct);
        if (product is null)
            return ValidationResult<BeginningInventoryEntryCreatedResult>.NotFound(
                $"Vật tư '{cmd.ProductCode}' không tồn tại.");

        try
        {
            var entry = BeginningInventoryEntry.Create(
                cmd.Period,
                warehouse.WarehouseId,
                product.ProductCode,
                cmd.UnitOfMeasure,
                cmd.BeginningQuantity,
                cmd.LotNumber,
                cmd.ExpirationDate,
                cmd.CreatedBy);

            await repo.AddAsync(entry, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<BeginningInventoryEntryCreatedResult>.Ok(
                new BeginningInventoryEntryCreatedResult(entry.EntryId));
        }
        catch (DomainException ex)
        {
            return ValidationResult<BeginningInventoryEntryCreatedResult>.Failure(ex.Message);
        }
    }
}
