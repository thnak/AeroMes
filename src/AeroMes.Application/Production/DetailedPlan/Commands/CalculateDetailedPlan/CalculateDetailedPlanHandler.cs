using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.CalculateDetailedPlan;

public class CalculateDetailedPlanHandler(
    IDetailedProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<CalculateDetailedPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CalculateDetailedPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdWithLinesAsync(cmd.DetailPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Detailed plan {cmd.DetailPlanId} not found.");

        try
        {
            var workingDays = GetWorkingDays(plan.PeriodStart, plan.PeriodEnd, cmd.WorkingDaysPerWeek);
            if (workingDays.Count == 0)
                return ValidationResult<Unit>.Failure("No working days in the specified period.");

            var slots = plan.Granularity == DppGranularity.Shift
                ? ExpandToShiftSlots(workingDays, cmd.ShiftLabels ?? ["Morning", "Afternoon"])
                : workingDays.Select(d => (d, (string?)null)).ToList();

            foreach (var line in plan.ProductLines)
            {
                line.ClearSlots();

                var allocations = AllocateSlots(
                    line.TotalRequiredQty, line.DailyCapacity,
                    cmd.Strategy, slots, plan.Granularity);

                foreach (var (date, shiftLabel, qty) in allocations)
                {
                    if (qty <= 0) continue;
                    var slot = DppSlot.Create(line.DppLineId, date, shiftLabel, qty);
                    line.AddSlot(slot);
                }
            }
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }

    private static List<DateOnly> GetWorkingDays(DateOnly start, DateOnly end, int workingDaysPerWeek)
    {
        var result = new List<DateOnly>();
        var current = start;
        while (current <= end)
        {
            // Working days are Mon–(Mon+workingDaysPerWeek-1), i.e., first N days of week
            var dow = (int)current.DayOfWeek; // 0=Sun
            var weekDayNo = dow == 0 ? 7 : dow; // 1=Mon..7=Sun
            if (weekDayNo <= workingDaysPerWeek)
                result.Add(current);
            current = current.AddDays(1);
        }
        return result;
    }

    private static List<(DateOnly Date, string? Shift)> ExpandToShiftSlots(
        List<DateOnly> workingDays, IReadOnlyList<string> shiftLabels)
    {
        var result = new List<(DateOnly, string?)>();
        foreach (var day in workingDays)
            foreach (var shift in shiftLabels)
                result.Add((day, shift));
        return result;
    }

    private static List<(DateOnly Date, string? Shift, decimal Qty)> AllocateSlots(
        decimal totalQty, decimal dailyCapacity,
        DppDistributionStrategy strategy,
        List<(DateOnly Date, string? Shift)> slots,
        DppGranularity granularity)
    {
        var result = new List<(DateOnly, string?, decimal)>();
        decimal remaining = totalQty;
        int slotCount = slots.Count;
        if (slotCount == 0) return result;

        // For shift granularity, capacity is split equally across shifts per day
        int shiftsPerDay = granularity == DppGranularity.Shift
            ? slots.GroupBy(s => s.Date).First().Count()
            : 1;
        decimal slotCapacity = dailyCapacity / shiftsPerDay;

        var orderedSlots = strategy == DppDistributionStrategy.Backward
            ? slots.AsEnumerable().Reverse().ToList()
            : slots;

        if (strategy == DppDistributionStrategy.EvenSpread)
        {
            decimal perSlot = Math.Min(totalQty / slotCount, slotCapacity);
            decimal runningTotal = 0;
            for (int i = 0; i < orderedSlots.Count; i++)
            {
                var (date, shift) = orderedSlots[i];
                var qty = i == orderedSlots.Count - 1
                    ? Math.Max(0, totalQty - runningTotal)
                    : Math.Min(perSlot, slotCapacity);
                qty = Math.Min(qty, slotCapacity);
                result.Add((date, shift, qty));
                runningTotal += qty;
            }
        }
        else
        {
            foreach (var (date, shift) in orderedSlots)
            {
                if (remaining <= 0) break;
                var qty = Math.Min(slotCapacity, remaining);
                result.Add((date, shift, qty));
                remaining -= qty;
            }
        }

        return result;
    }
}
