using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialRequisition;

public class UpdateMaterialRequisitionHandler(
    IMaterialRequisitionRepository requisitionRepo,
    IProductionOrderRepository productionOrderRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<UpdateMaterialRequisitionCommand> validator)
    : ICommandHandler<UpdateMaterialRequisitionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateMaterialRequisitionCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var requisition = await requisitionRepo.GetByIdWithLinesAsync(cmd.RequisitionId, ct);
        if (requisition is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu xuất '{cmd.RequisitionId}' không tồn tại.");

        if (cmd.ProductionOrderId.HasValue)
        {
            var po = await productionOrderRepo.GetByIdAsync(cmd.ProductionOrderId.Value, ct);
            if (po is null)
                return ValidationResult<Unit>.NotFound($"Lệnh sản xuất '{cmd.ProductionOrderId}' không tồn tại.");
        }

        foreach (var line in cmd.Lines)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(line.WarehouseId, ct);
            if (warehouse is null)
                return ValidationResult<Unit>.NotFound($"Kho '{line.WarehouseId}' không tồn tại.");

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Vật tư '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            requisition.UpdateHeader(cmd.ProductionOrderId, cmd.RequesterUnit, cmd.RequestDate, cmd.Notes, cmd.UpdatedBy);
            requisition.ClearLines();

            foreach (var line in cmd.Lines)
                requisition.AddLine(line.ProductCode, line.UnitOfMeasure, line.RequestedQuantity, line.WarehouseId, line.Notes);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
