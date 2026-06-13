using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateMaterialTransferSlip;

public class CreateMaterialTransferSlipHandler(
    IMaterialTransferSlipRepository slipRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateMaterialTransferSlipCommand> validator)
    : ICommandHandler<CreateMaterialTransferSlipCommand, ValidationResult<MaterialTransferSlipCreatedResult>>
{
    public async Task<ValidationResult<MaterialTransferSlipCreatedResult>> HandleAsync(
        CreateMaterialTransferSlipCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<MaterialTransferSlipCreatedResult>.Invalid(validation.ToErrorDictionary());

        var sourceWarehouse = await warehouseRepo.GetByIdAsync(cmd.SourceWarehouseId, ct);
        if (sourceWarehouse is null)
            return ValidationResult<MaterialTransferSlipCreatedResult>.NotFound(
                $"Kho nguồn '{cmd.SourceWarehouseId}' không tồn tại.");

        var destWarehouse = await warehouseRepo.GetByIdAsync(cmd.DestinationWarehouseId, ct);
        if (destWarehouse is null)
            return ValidationResult<MaterialTransferSlipCreatedResult>.NotFound(
                $"Kho đích '{cmd.DestinationWarehouseId}' không tồn tại.");

        foreach (var line in cmd.Lines)
        {
            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<MaterialTransferSlipCreatedResult>.NotFound(
                    $"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            var voucherNumber = $"DCK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            var slip = MaterialTransferSlip.Create(
                voucherNumber,
                cmd.TransferType,
                cmd.ReferenceRequestId,
                cmd.SourceWarehouseId,
                cmd.DestinationWarehouseId,
                cmd.Notes,
                cmd.CreatedBy);

            await slipRepo.AddAsync(slip, ct);
            await uow.SaveChangesAsync(ct);

            foreach (var line in cmd.Lines)
                slip.AddLine(line.ProductCode, line.UnitOfMeasure, line.Quantity, line.SpecificationCode);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<MaterialTransferSlipCreatedResult>.Ok(
                new MaterialTransferSlipCreatedResult(slip.SlipId, slip.VoucherNumber));
        }
        catch (DomainException ex)
        {
            return ValidationResult<MaterialTransferSlipCreatedResult>.Failure(ex.Message);
        }
    }
}
