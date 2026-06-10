using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Commands.DeleteWorkShift;

public class DeleteWorkShiftHandler(
    IWorkShiftRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkShiftCommand>
{
    public async Task HandleAsync(DeleteWorkShiftCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.WorkShiftId, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.WorkShiftId), cmd.WorkShiftId);
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
