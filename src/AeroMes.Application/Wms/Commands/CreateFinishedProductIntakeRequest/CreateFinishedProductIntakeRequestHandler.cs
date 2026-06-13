using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateFinishedProductIntakeRequest;

public class CreateFinishedProductIntakeRequestHandler(
    IFinishedProductIntakeRequestRepository intakeRepo,
    IProductionOrderRepository productionOrderRepo,
    IWarehouseRepository warehouseRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<CreateFinishedProductIntakeRequestCommand> validator)
    : ICommandHandler<CreateFinishedProductIntakeRequestCommand, ValidationResult<FinishedProductIntakeRequestCreatedResult>>
{
    public async Task<ValidationResult<FinishedProductIntakeRequestCreatedResult>> HandleAsync(
        CreateFinishedProductIntakeRequestCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<FinishedProductIntakeRequestCreatedResult>.Invalid(validation.ToErrorDictionary());

        if (cmd.ProductionOrderId.HasValue)
        {
            var po = await productionOrderRepo.GetByIdAsync(cmd.ProductionOrderId.Value, ct);
            if (po is null)
                return ValidationResult<FinishedProductIntakeRequestCreatedResult>.NotFound(
                    $"Lệnh sản xuất '{cmd.ProductionOrderId}' không tồn tại.");
        }

        foreach (var line in cmd.Lines)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(line.WarehouseId, ct);
            if (warehouse is null)
                return ValidationResult<FinishedProductIntakeRequestCreatedResult>.NotFound(
                    $"Kho '{line.WarehouseId}' không tồn tại.");

            var product = await productRepo.GetByCodeAsync(line.ProductCode, ct);
            if (product is null)
                return ValidationResult<FinishedProductIntakeRequestCreatedResult>.NotFound(
                    $"Vật tư/thành phẩm '{line.ProductCode}' không tồn tại.");
        }

        try
        {
            var number = $"YCNT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

            var request = FinishedProductIntakeRequest.Create(
                number,
                cmd.IntakePurpose,
                cmd.WarehouseType,
                cmd.ProductionOrderId,
                cmd.RequesterUnit,
                cmd.RequestDate,
                cmd.Notes,
                cmd.CreatedBy);

            await intakeRepo.AddAsync(request, ct);
            await uow.SaveChangesAsync(ct);

            foreach (var line in cmd.Lines)
                request.AddLine(line.ProductCode, line.UnitOfMeasure, line.RequestedQuantity, line.WarehouseId, line.IsDefective, line.DefectReason, line.Notes);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<FinishedProductIntakeRequestCreatedResult>.Ok(
                new FinishedProductIntakeRequestCreatedResult(request.IntakeRequestId, request.RequestNumber));
        }
        catch (DomainException ex)
        {
            return ValidationResult<FinishedProductIntakeRequestCreatedResult>.Failure(ex.Message);
        }
    }
}
