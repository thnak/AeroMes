using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialSupplyRequest;

public class UpdateMaterialSupplyRequestHandler(
    IMaterialSupplyRequestRepository requestRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<UpdateMaterialSupplyRequestCommand> validator)
    : ICommandHandler<UpdateMaterialSupplyRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateMaterialSupplyRequestCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var request = await requestRepo.GetByIdWithLinesAsync(cmd.RequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Phiếu đề nghị '{cmd.RequestId}' không tồn tại.");

        foreach (var line in cmd.Lines)
        {
            if (line.WarehouseId.HasValue)
            {
                var warehouse = await warehouseRepo.GetByIdAsync(line.WarehouseId.Value, ct);
                if (warehouse is null)
                    return ValidationResult<Unit>.NotFound($"Kho '{line.WarehouseId}' không tồn tại.");
            }

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            request.UpdateHeader(cmd.RequestType, cmd.RequesterUnit, cmd.RequiredByDate, cmd.Notes, cmd.UpdatedBy);
            request.ClearLines();

            foreach (var line in cmd.Lines)
                request.AddLine(line.ProductCode, line.UnitOfMeasure, line.RequestedQuantity, line.WarehouseId, line.Notes);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
