using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Schedule.Commands.DeleteSchedule;

public record DeleteScheduleCommand(int ScheduleId) : ICommand<ValidationResult<Unit>>;

public class DeleteScheduleCommandHandler(IProductionScheduleRepository repo)
    : ICommandHandler<DeleteScheduleCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteScheduleCommand cmd, CancellationToken ct = default)
    {
        var schedule = await repo.GetByIdAsync(cmd.ScheduleId, ct);
        if (schedule is null) return ValidationResult<Unit>.NotFound("Schedule not found.");
        if (schedule.Status == ScheduleStatus.Approved)
            return ValidationResult<Unit>.Failure("Cannot delete an approved schedule.");
        await repo.DeleteAsync(schedule, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
