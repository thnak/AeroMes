using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.DistributeMasterPlan;

public class DistributeMasterPlanHandler(
    IMasterProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<DistributeMasterPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DistributeMasterPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdWithLinesAsync(cmd.MasterPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Master plan {cmd.MasterPlanId} not found.");

        try
        {
            var subPeriods = GenerateSubPeriods(plan);
            if (subPeriods.Count == 0)
                return ValidationResult<Unit>.Failure("No valid sub-periods generated from the plan's granularity.");

            foreach (var line in plan.Lines)
            {
                var planned = DistributeLine(line.QuantityRequired, line.DailyCapacity,
                    cmd.Strategy, plan.WorkingDaysPerWeek, subPeriods);
                line.UpdatePlannedQuantity(planned);
            }
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }

    private static List<(DateOnly Start, DateOnly End)> GenerateSubPeriods(MasterProductionPlan plan)
    {
        var result = new List<(DateOnly, DateOnly)>();
        var current = plan.PeriodStart;

        while (current < plan.PeriodEnd)
        {
            DateOnly next = plan.Granularity switch
            {
                MpsGranularity.Week => current.AddDays(7),
                MpsGranularity.Month => new DateOnly(current.Year, current.Month, 1).AddMonths(1),
                MpsGranularity.Quarter => new DateOnly(current.Year, ((current.Month - 1) / 3) * 3 + 1, 1).AddMonths(3),
                _ => current.AddMonths(1)
            };
            var end = next < plan.PeriodEnd ? next.AddDays(-1) : plan.PeriodEnd;
            result.Add((current, end));
            current = next;
        }
        return result;
    }

    private static decimal DistributeLine(
        decimal requiredQty, decimal dailyCapacity,
        MpsDistributionStrategy strategy,
        int workingDaysPerWeek,
        List<(DateOnly Start, DateOnly End)> subPeriods)
    {
        // Calculate working days per sub-period and maximum capacity per sub-period
        var periodCapacities = subPeriods.Select(sp =>
        {
            var totalDays = (sp.End.ToDateTime(TimeOnly.MinValue) - sp.Start.ToDateTime(TimeOnly.MinValue)).Days + 1;
            var weeks = totalDays / 7.0;
            var workingDays = (int)Math.Round(weeks * workingDaysPerWeek);
            return Math.Min(dailyCapacity * workingDays, requiredQty);
        }).ToList();

        // Distribute according to strategy
        decimal remaining = requiredQty;
        var allocations = new decimal[subPeriods.Count];

        var indices = strategy == MpsDistributionStrategy.Backward
            ? Enumerable.Range(0, subPeriods.Count).Reverse().ToList()
            : Enumerable.Range(0, subPeriods.Count).ToList();

        foreach (var i in indices)
        {
            if (remaining <= 0) break;
            var alloc = Math.Min(periodCapacities[i], remaining);
            allocations[i] = alloc;
            remaining -= alloc;
        }

        // Return total planned (sum = what can be planned given capacity)
        return allocations.Sum();
    }
}
