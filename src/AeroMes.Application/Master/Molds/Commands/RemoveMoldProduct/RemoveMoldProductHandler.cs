using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.RemoveMoldProduct;

public class RemoveMoldProductHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveMoldProductCommand>
{
    public async Task HandleAsync(RemoveMoldProductCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        mold.RemoveProductMapping(cmd.MappingId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
