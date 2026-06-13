using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Schedule.Commands.AssignPrimaryCapacity;

public record AssignPrimaryCapacityCommand(int ScheduleId, int POID, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;

public class AssignPrimaryCapacityCommandHandler(IProductionScheduleRepository repo)
    : ICommandHandler<AssignPrimaryCapacityCommand, ValidationResult<Unit>>
{
    private const double DefaultCycleTimeSeconds = 60.0;

    public async Task<ValidationResult<Unit>> HandleAsync(
        AssignPrimaryCapacityCommand cmd, CancellationToken ct = default)
    {
        var schedule = await repo.GetByIdAsync(cmd.ScheduleId, ct);
        if (schedule is null) return ValidationResult<Unit>.NotFound("Schedule not found.");

        var pending = await repo.GetPendingOrdersAsync(
            schedule.PeriodStart, schedule.PeriodEnd, cmd.ScheduleId, ct);
        var order = pending.FirstOrDefault(o => o.POID == cmd.POID);
        if (order is null)
            return ValidationResult<Unit>.Failure($"Order {cmd.POID} is not a pending order for this schedule.");

        var capacity = await repo.GetPrimaryCapacityAsync(order.ProductCode, ct);
        if (capacity is null)
            return ValidationResult<Unit>.Failure("No routing configured for this product.");

        var cycleTime = capacity.CycleTimeSeconds > 0 ? capacity.CycleTimeSeconds : DefaultCycleTimeSeconds;
        var duration = TimeSpan.FromSeconds(order.TargetQuantity * cycleTime);

        // Find earliest available slot for this WorkCenter
        var slots = await repo.GetScheduledSlotsByWorkCenterAsync(
            schedule.PeriodStart, schedule.PeriodEnd, cmd.ScheduleId, ct);

        var start = schedule.PeriodStart;
        if (slots.TryGetValue(capacity.WorkCenterID, out var existing))
        {
            var lastEnd = existing.Count > 0 ? existing.Max(s => s.End) : schedule.PeriodStart;
            if (lastEnd > start) start = lastEnd;
        }

        var end = start + duration;
        if (end > schedule.PeriodEnd) end = schedule.PeriodEnd;

        var nextSeq = schedule.Lines.Count > 0 ? schedule.Lines.Max(l => l.SequenceNo) + 1 : 1;
        var newLine = ProductionScheduleLine.Create(
            cmd.ScheduleId, cmd.POID, capacity.WorkCenterID, start, end, nextSeq);

        var lines = schedule.Lines.ToList();
        lines.Add(newLine);
        schedule.SetLines(lines);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
