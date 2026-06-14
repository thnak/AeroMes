using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Schedule.Commands.ConfirmSchedule;

public record ConfirmScheduleCommand(int ScheduleId, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;

public class ConfirmScheduleHandler(
    IProductionScheduleRepository repo,
    ICapacityCalendarRepository capacityRepo)
    : ICommandHandler<ConfirmScheduleCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ConfirmScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = await repo.GetByIdAsync(cmd.ScheduleId, ct);
        if (schedule is null) return ValidationResult<Unit>.NotFound("Schedule not found.");

        try
        {
            // Load available minutes per work center for the schedule period
            var from = DateOnly.FromDateTime(schedule.PeriodStart);
            var to = DateOnly.FromDateTime(schedule.PeriodEnd);
            var usedMinutes = await repo.GetUsedMinutesPerWorkCenterAsync(cmd.ScheduleId, ct);

            // Check each work center's used minutes against capacity calendar
            var periodDays = (schedule.PeriodEnd - schedule.PeriodStart).TotalDays;
            foreach (var (wcId, usedMins) in usedMinutes)
            {
                var totalAvailable = 0;
                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    var dayCapacity = await capacityRepo.GetAvailableMinutesOnDateAsync(d, ct);
                    if (dayCapacity.TryGetValue(wcId, out var mins))
                        totalAvailable += mins;
                }
                if (totalAvailable > 0 && usedMins > totalAvailable)
                    throw new DomainException(
                        $"Work center {wcId} is over-allocated: {usedMins} min planned vs {totalAvailable} min available.");
            }

            schedule.Confirm(cmd.UpdatedBy ?? string.Empty);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
