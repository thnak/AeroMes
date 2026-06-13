using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.UpdateMasterPlan;

public class UpdateMasterPlanHandler(
    IMasterProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateMasterPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateMasterPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdWithLinesAsync(cmd.MasterPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Master plan {cmd.MasterPlanId} not found.");

        try
        {
            plan.Update(cmd.PlanName, cmd.OrganizationalUnit,
                cmd.WorkingHoursPerDay, cmd.WorkingDaysPerWeek, cmd.UpdatedBy);

            var lines = cmd.Lines.Select(l => MasterPlanLine.Create(
                plan.MasterPlanId, l.ProductCode, l.ProductName,
                l.UnitOfMeasure, l.QuantityRequired,
                l.DailyCapacity, l.OpeningInventory,
                l.DistributionStrategy)).ToList();

            plan.ReplaceLines(lines, cmd.UpdatedBy);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
