using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.UpdateWorkCalendar;

public class UpdateWorkCalendarHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateWorkCalendarCommand>
{
    public async Task HandleAsync(UpdateWorkCalendarCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdWithDetailsAsync(cmd.WorkCalendarId, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.WorkCalendarId), cmd.WorkCalendarId);
        entity.UpdateDetails(
            cmd.Name, cmd.Description, cmd.IsActive,
            cmd.Days.Select(d => (d.DayOfWeek, d.IsWorkingDay,
                d.Shifts.Select(s => (s.WorkShiftId, s.Sequence)).AsEnumerable())),
            cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
