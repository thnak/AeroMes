using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.UnmountMold;

public class UnmountMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<UnmountMoldCommand>
{
    public async Task HandleAsync(UnmountMoldCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        mold.Unmount(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
