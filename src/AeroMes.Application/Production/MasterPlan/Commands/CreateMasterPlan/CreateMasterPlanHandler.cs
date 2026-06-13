using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.CreateMasterPlan;

public class CreateMasterPlanHandler(
    IMasterProductionPlanRepository repo,
    IUnitOfWork uow,
    IValidator<CreateMasterPlanCommand> validator) : ICommandHandler<CreateMasterPlanCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateMasterPlanCommand cmd, CancellationToken ct = default)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid)
            return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var planNumber = string.IsNullOrWhiteSpace(cmd.PlanNumber)
            ? await repo.NextPlanNumberAsync(ct)
            : cmd.PlanNumber.Trim().ToUpperInvariant();

        if (await repo.ExistsByPlanNumberAsync(planNumber, ct))
            return ValidationResult<int>.Failure($"Plan number '{planNumber}' already exists.");

        var plan = MasterProductionPlan.Create(
            planNumber, cmd.PlanName, cmd.OrganizationalUnit,
            cmd.Granularity, cmd.PeriodStart, cmd.PeriodEnd,
            cmd.DataSource, cmd.WorkingHoursPerDay, cmd.WorkingDaysPerWeek,
            cmd.CreatedBy);

        await repo.AddAsync(plan, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var lineInput in cmd.Lines)
        {
            var line = MasterPlanLine.Create(
                plan.MasterPlanId, lineInput.ProductCode, lineInput.ProductName,
                lineInput.UnitOfMeasure, lineInput.QuantityRequired,
                lineInput.DailyCapacity, lineInput.OpeningInventory,
                lineInput.DistributionStrategy);
            plan.AddLine(line);
        }

        if (cmd.Lines.Count > 0)
            await uow.SaveChangesAsync(ct);

        return ValidationResult<int>.Ok(plan.MasterPlanId);
    }
}
