using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateFinishedProductIntakeRequest;

public class UpdateFinishedProductIntakeRequestHandler(
    IFinishedProductIntakeRequestRepository intakeRepo,
    IProductionOrderRepository productionOrderRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<UpdateFinishedProductIntakeRequestCommand> validator)
    : ICommandHandler<UpdateFinishedProductIntakeRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateFinishedProductIntakeRequestCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var request = await intakeRepo.GetByIdWithLinesAsync(cmd.IntakeRequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu nhập '{cmd.IntakeRequestId}' không tồn tại.");

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
                return ValidationResult<Unit>.NotFound($"Vật tư/thành phẩm '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            request.UpdateHeader(cmd.IntakePurpose, cmd.WarehouseType, cmd.ProductionOrderId, cmd.RequesterUnit, cmd.RequestDate, cmd.Notes, cmd.UpdatedBy);
            request.ClearLines();

            foreach (var line in cmd.Lines)
                request.AddLine(line.ProductCode, line.UnitOfMeasure, line.RequestedQuantity, line.WarehouseId, line.IsDefective, line.DefectReason, line.Notes);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
