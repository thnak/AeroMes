using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCenters.Commands.CreateWorkCenter;

public class CreateWorkCenterHandler(
    IWorkCenterRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateWorkCenterCommand, int>
{
    public async Task<int> HandleAsync(CreateWorkCenterCommand cmd, CancellationToken ct)
    {
        var entity = WorkCenter.Create(cmd.Code, cmd.Name, cmd.Description, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.WorkCenterID;
    }
}
