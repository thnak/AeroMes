using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.DeleteWorkCalendar;

public class DeleteWorkCalendarHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkCalendarCommand>
{
    public async Task HandleAsync(DeleteWorkCalendarCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.WorkCalendarId, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.WorkCalendarId), cmd.WorkCalendarId);
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
