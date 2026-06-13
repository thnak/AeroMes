using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateMaterialRequisition;

public class CreateMaterialRequisitionHandler(
    IMaterialRequisitionRepository requisitionRepo,
    IProductionOrderRepository productionOrderRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateMaterialRequisitionCommand> validator)
    : ICommandHandler<CreateMaterialRequisitionCommand, ValidationResult<MaterialRequisitionCreatedResult>>
{
    public async Task<ValidationResult<MaterialRequisitionCreatedResult>> HandleAsync(
        CreateMaterialRequisitionCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<MaterialRequisitionCreatedResult>.Invalid(validation.ToErrorDictionary());

        if (cmd.ProductionOrderId.HasValue)
        {
            var po = await productionOrderRepo.GetByIdAsync(cmd.ProductionOrderId.Value, ct);
            if (po is null)
                return ValidationResult<MaterialRequisitionCreatedResult>.NotFound(
                    $"Lệnh sản xuất '{cmd.ProductionOrderId}' không tồn tại.");
        }

        foreach (var line in cmd.Lines)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(line.WarehouseId, ct);
            if (warehouse is null)
                return ValidationResult<MaterialRequisitionCreatedResult>.NotFound(
                    $"Kho '{line.WarehouseId}' không tồn tại.");

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<MaterialRequisitionCreatedResult>.NotFound(
                    $"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            var number = $"YCX-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            var requisition = MaterialRequisition.Create(
                number,
                cmd.ProductionOrderId,
                cmd.RequesterUnit,
                cmd.RequestDate,
                cmd.Notes,
                cmd.CreatedBy);

            await requisitionRepo.AddAsync(requisition, ct);
            await uow.SaveChangesAsync(ct);

            foreach (var line in cmd.Lines)
                requisition.AddLine(line.ProductCode, line.UnitOfMeasure, line.RequestedQuantity, line.WarehouseId, line.Notes);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<MaterialRequisitionCreatedResult>.Ok(
                new MaterialRequisitionCreatedResult(requisition.RequisitionId, requisition.RequisitionNumber));
        }
        catch (DomainException ex)
        {
            return ValidationResult<MaterialRequisitionCreatedResult>.Failure(ex.Message);
        }
    }
}
