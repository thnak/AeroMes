using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.EngChanges.Commands.SubmitEngChangeForReview;

public class SubmitEngChangeForReviewHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow) : ICommandHandler<SubmitEngChangeForReviewCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(SubmitEngChangeForReviewCommand cmd, CancellationToken ct)
    {
        var ec = await repo.GetByNumberAsync(cmd.EcNumber, ct);
        if (ec is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.EcNumber}' was not found.");

        ec.SubmitForReview(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
