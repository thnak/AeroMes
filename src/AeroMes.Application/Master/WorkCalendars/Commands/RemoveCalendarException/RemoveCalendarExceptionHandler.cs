using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkCalendars.Commands.RemoveCalendarException;

public class RemoveCalendarExceptionHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveCalendarExceptionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveCalendarExceptionCommand cmd, CancellationToken ct)
    {
        var calendar = await repo.GetByIdWithDetailsAsync(cmd.WorkCalendarId, ct);
        if (calendar is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.WorkCalendarId}' was not found.");
        calendar.RemoveException(cmd.ExceptionId);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
