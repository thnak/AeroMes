using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseReceipt;

public class UpdateFactoryWarehouseReceiptHandler(
    IFactoryWarehouseReceiptRepository receiptRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<UpdateFactoryWarehouseReceiptCommand> validator)
    : ICommandHandler<UpdateFactoryWarehouseReceiptCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateFactoryWarehouseReceiptCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var receipt = await receiptRepo.GetByIdWithLinesAsync(cmd.ReceiptId, ct);
        if (receipt is null)
            return ValidationResult<Unit>.NotFound($"Phiếu nhập '{cmd.ReceiptId}' không tồn tại.");

        foreach (var line in cmd.Lines)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(line.DestinationWarehouseId, ct);
            if (warehouse is null)
                return ValidationResult<Unit>.NotFound($"Kho '{line.DestinationWarehouseId}' không tồn tại.");

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            receipt.UpdateHeader(cmd.ReceiptType, cmd.ReferenceRequestId, cmd.SupplierOrTransferringUnit, cmd.Notes, cmd.UpdatedBy);
            receipt.ClearLines();

            foreach (var line in cmd.Lines)
                receipt.AddLine(line.ProductCode, line.UnitOfMeasure, line.Quantity, line.DestinationWarehouseId, line.SpecificationCode);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
