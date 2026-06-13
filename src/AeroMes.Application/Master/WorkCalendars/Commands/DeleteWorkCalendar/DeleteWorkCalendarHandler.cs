using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkCalendars.Commands.DeleteWorkCalendar;

public class DeleteWorkCalendarHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkCalendarCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteWorkCalendarCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.WorkCalendarId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.WorkCalendarId}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
