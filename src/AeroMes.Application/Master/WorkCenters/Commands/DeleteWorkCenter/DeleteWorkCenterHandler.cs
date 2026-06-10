using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCenters.Commands.DeleteWorkCenter;

public class DeleteWorkCenterHandler(
    IWorkCenterRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteWorkCenterCommand>
{
    public async Task HandleAsync(DeleteWorkCenterCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("WorkCenter", cmd.Id);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
