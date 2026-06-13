using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ApproveCycleCount;

public class ApproveCycleCountHandler(
    ICycleCountPlanRepository planRepo,
    IInventoryStockRepository stockRepo,
    IStockMovementRepository movementRepo,
    IUnitOfWork uow,
    IValidator<ApproveCycleCountCommand> validator)
    : ICommandHandler<ApproveCycleCountCommand, ValidationResult<ApproveCycleCountResult>>
{
    public async Task<ValidationResult<ApproveCycleCountResult>> HandleAsync(
        ApproveCycleCountCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<ApproveCycleCountResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var plan = await planRepo.GetByIdWithLinesAsync(cmd.PlanId, ct);
            if (plan is null)
                return ValidationResult<ApproveCycleCountResult>.NotFound(
                    $"Không tìm thấy kế hoạch kiểm kê #{cmd.PlanId}.");

            var approvedCount = 0;
            var rejectedCount = 0;

            foreach (var line in plan.Lines.Where(l => l.Status == CycleCountLineStatus.Counted).ToList())
            {
                var absVariancePct = line.VariancePct.HasValue ? Math.Abs(line.VariancePct.Value) : 0m;

                if (absVariancePct > cmd.VarianceThresholdPct)
                {
                    plan.RejectLineForRecount(line.LineId);
                    rejectedCount++;
                }
                else
                {
                    plan.ApproveLine(line.LineId);
                    approvedCount++;

                    var delta = line.VarianceQty ?? 0m;
                    if (delta != 0m)
                    {
                        var stock = await stockRepo.FindByKeyAsync(
                            line.LocationId, line.ProductCode, line.LotNumber, ct);
                        if (stock is not null)
                        {
                            stock.Adjust(delta);
                            var movement = StockMovement.CreateAdjust(
                                line.ProductCode, line.LotNumber, delta,
                                line.LocationId, line.BinId,
                                plan.PlanCode,
                                cmd.Notes,
                                cmd.ApprovedBy);
                            await movementRepo.AddAsync(movement, ct);
                        }
                    }
                }
            }

            if (rejectedCount > 0)
                plan.RevertToInProgress();
            else
                plan.CompletePlan(cmd.ApprovedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<ApproveCycleCountResult>.Ok(new(approvedCount, rejectedCount));
        }
        catch (DomainException ex)
        {
            return ValidationResult<ApproveCycleCountResult>.Failure(ex.Message);
        }
    }
}
