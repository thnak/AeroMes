using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.WorkCenters.Commands.UpdateWorkCenter;

public class UpdateWorkCenterHandler(
    IWorkCenterRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateWorkCenterCommand>
{
    public async Task Handle(UpdateWorkCenterCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("WorkCenter", cmd.Id);
        entity.UpdateDetails(cmd.Name, cmd.Description, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
