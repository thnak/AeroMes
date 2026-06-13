using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseExport;

public class UpdateFactoryWarehouseExportHandler(
    IFactoryWarehouseExportRepository exportRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<UpdateFactoryWarehouseExportCommand> validator)
    : ICommandHandler<UpdateFactoryWarehouseExportCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateFactoryWarehouseExportCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var export = await exportRepo.GetByIdWithLinesAsync(cmd.ExportId, ct);
        if (export is null)
            return ValidationResult<Unit>.NotFound($"Phiếu xuất '{cmd.ExportId}' không tồn tại.");

        foreach (var line in cmd.Lines)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(line.SourceWarehouseId, ct);
            if (warehouse is null)
                return ValidationResult<Unit>.NotFound($"Kho '{line.SourceWarehouseId}' không tồn tại.");

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            export.UpdateHeader(cmd.ExportType, cmd.ReferenceRequestId, cmd.ReceiverOrReceivingUnit, cmd.Notes, cmd.UpdatedBy);
            export.ClearLines();

            foreach (var line in cmd.Lines)
                export.AddLine(line.ProductCode, line.UnitOfMeasure, line.Quantity, line.SourceWarehouseId, line.SpecificationCode);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
