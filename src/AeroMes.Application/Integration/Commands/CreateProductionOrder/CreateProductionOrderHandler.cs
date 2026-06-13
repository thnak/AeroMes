using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.CreateProductionOrder;

public sealed class CreateProductionOrderHandler(
    IProductionOrderRepository poRepo,
    IBomItemRepository bomRepo,
    IRoutingRepository routingRepo,
    IUnitOfWork uow)
    : ICommandHandler<CreateProductionOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateProductionOrderCommand cmd, CancellationToken ct = default)
    {
        if (cmd.TargetQuantity <= 0)
            return ValidationResult<int>.Failure("Số lượng sản xuất phải lớn hơn 0.");
        if (string.IsNullOrWhiteSpace(cmd.ProductCode))
            return ValidationResult<int>.Failure("Mã sản phẩm không được để trống.");

        var poCode = await poRepo.NextPoCodeAsync(ct);

        var po = ProductionOrder.CreateInternal(
            poCode, cmd.ProductCode, cmd.TargetQuantity,
            cmd.PlannedStartDate, cmd.PlannedEndDate, cmd.Deadline,
            cmd.Priority, cmd.AssignedTo, cmd.CreatedBy, cmd.SoId);

        await poRepo.AddAsync(po, ct);
        await uow.SaveChangesAsync(ct);   // flush to get POID assigned

        if (cmd.AutoExpandBom)
        {
            var bomItems = await bomRepo.GetByParentAsync(cmd.ProductCode.Trim().ToUpperInvariant(), ct);
            foreach (var item in bomItems.Where(b => b.IsActive))
            {
                var stdQty = item.RequiredQty * cmd.TargetQuantity * (1m + item.ScrapFactor / 100m);
                po.AddMaterialLine(item.ChildProductCode, stdQty, string.Empty);
            }
        }

        if (cmd.RoutingId.HasValue)
        {
            var routing = await routingRepo.GetByIdWithStepsAsync(cmd.RoutingId.Value, ct);
            if (routing is not null)
            {
                foreach (var step in routing.Steps.OrderBy(s => s.StepNumber))
                    po.AddStage(step.StepNumber, step.OperationCode, step.DefaultWorkCenter?.WorkCenterCode);
            }
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(po.POID);
    }
}
