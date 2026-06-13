using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateFactoryWarehouseReceipt;

public class CreateFactoryWarehouseReceiptHandler(
    IFactoryWarehouseReceiptRepository receiptRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateFactoryWarehouseReceiptCommand> validator)
    : ICommandHandler<CreateFactoryWarehouseReceiptCommand, ValidationResult<FactoryWarehouseReceiptCreatedResult>>
{
    public async Task<ValidationResult<FactoryWarehouseReceiptCreatedResult>> HandleAsync(
        CreateFactoryWarehouseReceiptCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<FactoryWarehouseReceiptCreatedResult>.Invalid(validation.ToErrorDictionary());

        // Validate all destination warehouses and products
        foreach (var line in cmd.Lines)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(line.DestinationWarehouseId, ct);
            if (warehouse is null)
                return ValidationResult<FactoryWarehouseReceiptCreatedResult>.NotFound(
                    $"Kho '{line.DestinationWarehouseId}' không tồn tại.");

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<FactoryWarehouseReceiptCreatedResult>.NotFound(
                    $"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            var voucherNumber = $"FNK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            var receipt = FactoryWarehouseReceipt.Create(
                voucherNumber,
                cmd.ReceiptType,
                cmd.ReferenceRequestId,
                cmd.SupplierOrTransferringUnit,
                cmd.Notes,
                cmd.CreatedBy);

            await receiptRepo.AddAsync(receipt, ct);
            await uow.SaveChangesAsync(ct);

            // Add lines after save so ReceiptId is populated
            foreach (var line in cmd.Lines)
                receipt.AddLine(line.ProductCode, line.UnitOfMeasure, line.Quantity, line.DestinationWarehouseId, line.SpecificationCode);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<FactoryWarehouseReceiptCreatedResult>.Ok(
                new FactoryWarehouseReceiptCreatedResult(receipt.ReceiptId, receipt.VoucherNumber));
        }
        catch (DomainException ex)
        {
            return ValidationResult<FactoryWarehouseReceiptCreatedResult>.Failure(ex.Message);
        }
    }
}
