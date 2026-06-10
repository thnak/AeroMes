using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;

public class CreateStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateStorageLocationCommand, int>
{
    public async Task<int> HandleAsync(CreateStorageLocationCommand cmd, CancellationToken ct)
    {
        var entity = StorageLocation.Create(cmd.Code, cmd.Name, cmd.LocationType, cmd.WorkCenterId);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.LocationID;
    }
}
