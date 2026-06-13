using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Schedule.Commands.CompleteSchedule;

public record CompleteScheduleCommand(int ScheduleId, string? UpdatedBy, bool SaveAsDraft = false)
    : ICommand<ValidationResult<Unit>>;

public class CompleteScheduleCommandHandler(IProductionScheduleRepository repo)
    : ICommandHandler<CompleteScheduleCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CompleteScheduleCommand cmd, CancellationToken ct = default)
    {
        var schedule = await repo.GetByIdAsync(cmd.ScheduleId, ct);
        if (schedule is null) return ValidationResult<Unit>.NotFound("Schedule not found.");
        try
        {
            if (cmd.SaveAsDraft) schedule.SaveAsDraft(cmd.UpdatedBy ?? string.Empty);
            else schedule.Complete(cmd.UpdatedBy ?? string.Empty);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex) { return ValidationResult<Unit>.Failure(ex.Message); }
    }
}
