using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.SubmitBomForReview;

public class SubmitBomForReviewHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow) : ICommandHandler<SubmitBomForReviewCommand>
{
    public async Task HandleAsync(SubmitBomForReviewCommand cmd, CancellationToken ct)
    {
        var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct)
            ?? throw new EntityNotFoundException(nameof(BomHeader), $"{cmd.ProductCode}/{cmd.Version}");

        header.SubmitForReview(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
