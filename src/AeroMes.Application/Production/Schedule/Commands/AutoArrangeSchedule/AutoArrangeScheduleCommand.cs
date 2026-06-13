using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Schedule.Commands.AutoArrangeSchedule;

public record AutoArrangeScheduleCommand(int ScheduleId, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;

public class AutoArrangeScheduleCommandHandler(IProductionScheduleRepository repo)
    : ICommandHandler<AutoArrangeScheduleCommand, ValidationResult<Unit>>
{
    private const double DefaultCycleTimeSeconds = 60.0; // fallback: 1 min/unit

    public async Task<ValidationResult<Unit>> HandleAsync(
        AutoArrangeScheduleCommand cmd, CancellationToken ct = default)
    {
        var schedule = await repo.GetByIdAsync(cmd.ScheduleId, ct);
        if (schedule is null) return ValidationResult<Unit>.NotFound("Schedule not found.");

        var pending = await repo.GetPendingOrdersAsync(
            schedule.PeriodStart, schedule.PeriodEnd, cmd.ScheduleId, ct);
        if (pending.Count == 0)
            return ValidationResult<Unit>.Failure("No pending orders in this period.");

        // Track next available start per WorkCenter
        var nextAvailable = new Dictionary<int, DateTime>();

        var newLines = new List<ProductionScheduleLine>();
        var seq = 1;

        // Sort by priority asc (lower = higher priority), then deadline asc
        var ordered = pending
            .OrderBy(o => o.Priority)
            .ThenBy(o => o.ProductionDeadline ?? o.PlannedEnd ?? schedule.PeriodEnd);

        foreach (var order in ordered)
        {
            var capacity = await repo.GetPrimaryCapacityAsync(order.ProductCode, ct);
            if (capacity is null) continue; // skip if no routing configured

            var cycleTime = capacity.CycleTimeSeconds > 0
                ? capacity.CycleTimeSeconds : DefaultCycleTimeSeconds;

            var durationSeconds = order.TargetQuantity * cycleTime;
            var duration = TimeSpan.FromSeconds(durationSeconds);

            var start = nextAvailable.TryGetValue(capacity.WorkCenterID, out var next)
                ? next : schedule.PeriodStart;
            if (start < schedule.PeriodStart) start = schedule.PeriodStart;

            var end = start + duration;
            if (end > schedule.PeriodEnd) end = schedule.PeriodEnd; // cap at period end

            newLines.Add(ProductionScheduleLine.Create(
                cmd.ScheduleId, order.POID, capacity.WorkCenterID, start, end, seq++));

            nextAvailable[capacity.WorkCenterID] = end;
        }

        schedule.SetLines(newLines);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
