using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Commands.UpdateWorkShift;

public class UpdateWorkShiftHandler(
    IWorkShiftRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateWorkShiftCommand>
{
    public async Task HandleAsync(UpdateWorkShiftCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdWithBreaksAsync(cmd.WorkShiftId, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.WorkShiftId), cmd.WorkShiftId);
        entity.UpdateDetails(
            cmd.Name, cmd.StartTime, cmd.EndTime,
            cmd.Breaks.Select(b => (b.BreakStart, b.BreakEnd)),
            cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
