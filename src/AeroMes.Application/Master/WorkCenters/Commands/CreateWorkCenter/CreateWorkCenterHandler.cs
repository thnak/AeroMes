using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.WorkCenters.Commands.CreateWorkCenter;

public class CreateWorkCenterHandler(
    IWorkCenterRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateWorkCenterCommand, int>
{
    public async Task<int> Handle(CreateWorkCenterCommand cmd, CancellationToken ct)
    {
        var entity = WorkCenter.Create(cmd.Code, cmd.Name, cmd.Description, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.WorkCenterID;
    }
}
