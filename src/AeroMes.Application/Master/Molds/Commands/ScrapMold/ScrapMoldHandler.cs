using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.ScrapMold;

public class ScrapMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<ScrapMoldCommand>
{
    public async Task HandleAsync(ScrapMoldCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        mold.Scrap(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
