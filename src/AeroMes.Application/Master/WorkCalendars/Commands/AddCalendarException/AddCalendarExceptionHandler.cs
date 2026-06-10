using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.AddCalendarException;

public class AddCalendarExceptionHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddCalendarExceptionCommand, int>
{
    public async Task<int> HandleAsync(AddCalendarExceptionCommand cmd, CancellationToken ct)
    {
        var calendar = await repo.GetByIdWithDetailsAsync(cmd.WorkCalendarId, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.WorkCalendarId), cmd.WorkCalendarId);
        var ex = calendar.AddException(cmd.Date, cmd.ExceptionType, cmd.WorkShiftId, cmd.CreatedBy);
        await uow.SaveChangesAsync(ct);
        return ex.CalendarExceptionId;
    }
}
