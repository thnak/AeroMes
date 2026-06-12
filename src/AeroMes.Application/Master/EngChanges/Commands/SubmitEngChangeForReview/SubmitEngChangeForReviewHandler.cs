using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.SubmitEngChangeForReview;

public class SubmitEngChangeForReviewHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow) : ICommandHandler<SubmitEngChangeForReviewCommand>
{
    public async Task HandleAsync(SubmitEngChangeForReviewCommand cmd, CancellationToken ct)
    {
        var ec = await repo.GetByNumberAsync(cmd.EcNumber, ct)
            ?? throw new EntityNotFoundException(nameof(EngChange), cmd.EcNumber);

        ec.SubmitForReview(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
