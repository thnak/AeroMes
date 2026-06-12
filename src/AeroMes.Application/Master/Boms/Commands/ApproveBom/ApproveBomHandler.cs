using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.ApproveBom;

public class ApproveBomHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow) : ICommandHandler<ApproveBomCommand>
{
    public async Task HandleAsync(ApproveBomCommand cmd, CancellationToken ct)
    {
        var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct)
            ?? throw new EntityNotFoundException(nameof(BomHeader), $"{cmd.ProductCode}/{cmd.Version}");

        header.Approve(cmd.ApprovedBy);
        await uow.SaveChangesAsync(ct);
    }
}
