using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.CreateWorkCalendar;

public class CreateWorkCalendarHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateWorkCalendarCommand, int>
{
    public async Task<int> HandleAsync(CreateWorkCalendarCommand cmd, CancellationToken ct)
    {
        var entity = WorkCalendar.Create(
            cmd.Code, cmd.Name, cmd.Description,
            cmd.Days.Select(d => (d.DayOfWeek, d.IsWorkingDay,
                d.Shifts.Select(s => (s.WorkShiftId, s.Sequence)).AsEnumerable())),
            cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.WorkCalendarId;
    }
}
