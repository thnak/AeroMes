using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateFactoryWarehouseExport;

public class CreateFactoryWarehouseExportHandler(
    IFactoryWarehouseExportRepository exportRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateFactoryWarehouseExportCommand> validator)
    : ICommandHandler<CreateFactoryWarehouseExportCommand, ValidationResult<FactoryWarehouseExportCreatedResult>>
{
    public async Task<ValidationResult<FactoryWarehouseExportCreatedResult>> HandleAsync(
        CreateFactoryWarehouseExportCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<FactoryWarehouseExportCreatedResult>.Invalid(validation.ToErrorDictionary());

        foreach (var line in cmd.Lines)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(line.SourceWarehouseId, ct);
            if (warehouse is null)
                return ValidationResult<FactoryWarehouseExportCreatedResult>.NotFound(
                    $"Kho '{line.SourceWarehouseId}' không tồn tại.");

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<FactoryWarehouseExportCreatedResult>.NotFound(
                    $"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            var voucherNumber = $"FXK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            var export = FactoryWarehouseExport.Create(
                voucherNumber,
                cmd.ExportType,
                cmd.ReferenceRequestId,
                cmd.ReceiverOrReceivingUnit,
                cmd.Notes,
                cmd.CreatedBy);

            await exportRepo.AddAsync(export, ct);
            await uow.SaveChangesAsync(ct);

            foreach (var line in cmd.Lines)
                export.AddLine(line.ProductCode, line.UnitOfMeasure, line.Quantity, line.SourceWarehouseId, line.SpecificationCode);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<FactoryWarehouseExportCreatedResult>.Ok(
                new FactoryWarehouseExportCreatedResult(export.ExportId, export.VoucherNumber));
        }
        catch (DomainException ex)
        {
            return ValidationResult<FactoryWarehouseExportCreatedResult>.Failure(ex.Message);
        }
    }
}
