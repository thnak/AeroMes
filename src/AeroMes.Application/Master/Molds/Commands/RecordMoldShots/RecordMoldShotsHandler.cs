using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.RecordMoldShots;

public class RecordMoldShotsHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<RecordMoldShotsCommand, RecordMoldShotsResult>
{
    public async Task<RecordMoldShotsResult> HandleAsync(RecordMoldShotsCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        mold.AccumulateShots(cmd.Shots, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);

        return new RecordMoldShotsResult(
            mold.CurrentShots, mold.MaxShots, mold.IsPmDue, mold.IsNearingEndOfLife);
    }
}
