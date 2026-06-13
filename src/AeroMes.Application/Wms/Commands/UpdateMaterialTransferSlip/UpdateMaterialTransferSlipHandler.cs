using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialTransferSlip;

public class UpdateMaterialTransferSlipHandler(
    IMaterialTransferSlipRepository slipRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<UpdateMaterialTransferSlipCommand> validator)
    : ICommandHandler<UpdateMaterialTransferSlipCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateMaterialTransferSlipCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var slip = await slipRepo.GetByIdWithLinesAsync(cmd.SlipId, ct);
        if (slip is null)
            return ValidationResult<Unit>.NotFound($"Phiếu điều chuyển '{cmd.SlipId}' không tồn tại.");

        var sourceWarehouse = await warehouseRepo.GetByIdAsync(cmd.SourceWarehouseId, ct);
        if (sourceWarehouse is null)
            return ValidationResult<Unit>.NotFound($"Kho nguồn '{cmd.SourceWarehouseId}' không tồn tại.");

        var destWarehouse = await warehouseRepo.GetByIdAsync(cmd.DestinationWarehouseId, ct);
        if (destWarehouse is null)
            return ValidationResult<Unit>.NotFound($"Kho đích '{cmd.DestinationWarehouseId}' không tồn tại.");

        foreach (var line in cmd.Lines)
        {
            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            slip.UpdateHeader(cmd.TransferType, cmd.ReferenceRequestId,
                cmd.SourceWarehouseId, cmd.DestinationWarehouseId, cmd.Notes, cmd.UpdatedBy);
            slip.ClearLines();

            foreach (var line in cmd.Lines)
                slip.AddLine(line.ProductCode, line.UnitOfMeasure, line.Quantity, line.SpecificationCode);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
