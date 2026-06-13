using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateMaterialSupplyRequest;

public class CreateMaterialSupplyRequestHandler(
    IMaterialSupplyRequestRepository requestRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateMaterialSupplyRequestCommand> validator)
    : ICommandHandler<CreateMaterialSupplyRequestCommand, ValidationResult<MaterialSupplyRequestCreatedResult>>
{
    public async Task<ValidationResult<MaterialSupplyRequestCreatedResult>> HandleAsync(
        CreateMaterialSupplyRequestCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<MaterialSupplyRequestCreatedResult>.Invalid(validation.ToErrorDictionary());

        foreach (var line in cmd.Lines)
        {
            if (line.WarehouseId.HasValue)
            {
                var warehouse = await warehouseRepo.GetByIdAsync(line.WarehouseId.Value, ct);
                if (warehouse is null)
                    return ValidationResult<MaterialSupplyRequestCreatedResult>.NotFound(
                        $"Kho '{line.WarehouseId}' không tồn tại.");
            }

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<MaterialSupplyRequestCreatedResult>.NotFound(
                    $"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            var voucherNumber = $"DNK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            var request = MaterialSupplyRequest.Create(
                voucherNumber,
                cmd.RequestType,
                cmd.RequesterUnit,
                cmd.RequiredByDate,
                cmd.Notes,
                cmd.CreatedBy);

            await requestRepo.AddAsync(request, ct);
            await uow.SaveChangesAsync(ct);

            foreach (var line in cmd.Lines)
                request.AddLine(line.ProductCode, line.UnitOfMeasure, line.RequestedQuantity, line.WarehouseId, line.Notes);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<MaterialSupplyRequestCreatedResult>.Ok(
                new MaterialSupplyRequestCreatedResult(request.RequestId, request.VoucherNumber));
        }
        catch (DomainException ex)
        {
            return ValidationResult<MaterialSupplyRequestCreatedResult>.Failure(ex.Message);
        }
    }
}
