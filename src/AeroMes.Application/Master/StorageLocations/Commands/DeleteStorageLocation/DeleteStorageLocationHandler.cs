using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;

public class DeleteStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteStorageLocationCommand>
{
    public async Task Handle(DeleteStorageLocationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("StorageLocation", cmd.Id);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
