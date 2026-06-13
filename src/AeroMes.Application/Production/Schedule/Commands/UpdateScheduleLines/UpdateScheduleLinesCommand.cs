using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Schedule.Commands.UpdateScheduleLines;

public record ScheduleLineInput(
    int POID, int WorkCenterID,
    DateTime PlannedStart, DateTime PlannedEnd, int SequenceNo, string? Notes);

public record UpdateScheduleLinesCommand(
    int ScheduleId, IReadOnlyList<ScheduleLineInput> Lines, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;

public class UpdateScheduleLinesCommandHandler(IProductionScheduleRepository repo)
    : ICommandHandler<UpdateScheduleLinesCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateScheduleLinesCommand cmd, CancellationToken ct = default)
    {
        var schedule = await repo.GetByIdAsync(cmd.ScheduleId, ct);
        if (schedule is null) return ValidationResult<Unit>.NotFound("Schedule not found.");

        try
        {
            var lines = cmd.Lines.Select((l, i) =>
                ProductionScheduleLine.Create(cmd.ScheduleId, l.POID, l.WorkCenterID,
                    l.PlannedStart, l.PlannedEnd, l.SequenceNo == 0 ? i + 1 : l.SequenceNo, l.Notes))
                .ToList();
            schedule.SetLines(lines);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
