using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.RemoveCalendarException;

public class RemoveCalendarExceptionHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveCalendarExceptionCommand>
{
    public async Task HandleAsync(RemoveCalendarExceptionCommand cmd, CancellationToken ct)
    {
        var calendar = await repo.GetByIdWithDetailsAsync(cmd.WorkCalendarId, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.WorkCalendarId), cmd.WorkCalendarId);
        calendar.RemoveException(cmd.ExceptionId);
        await uow.SaveChangesAsync(ct);
    }
}
