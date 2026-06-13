using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.UpdateDetailedPlan;

public class UpdateDetailedPlanHandler(
    IDetailedProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateDetailedPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateDetailedPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdWithLinesAsync(cmd.DetailPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Detailed plan {cmd.DetailPlanId} not found.");

        try
        {
            plan.Update(cmd.PlanName, cmd.Granularity, cmd.UpdatedBy);

            var lines = cmd.ProductLines.Select(l => DppProductLine.Create(
                plan.DetailPlanId, l.ProductCode, l.ProductName,
                l.UnitOfMeasure, l.TotalRequiredQty, l.DailyCapacity)).ToList();

            plan.ReplaceProductLines(lines, cmd.UpdatedBy);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
