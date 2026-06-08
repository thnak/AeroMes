using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;

public class CreateStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateStorageLocationCommand, int>
{
    public async Task<int> Handle(CreateStorageLocationCommand cmd, CancellationToken ct)
    {
        var entity = StorageLocation.Create(cmd.Code, cmd.Name, cmd.LocationType, cmd.WorkCenterId);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.LocationID;
    }
}
