using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.EngChanges.Commands.ApproveEngChange;

public class ApproveEngChangeHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow) : ICommandHandler<ApproveEngChangeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ApproveEngChangeCommand cmd, CancellationToken ct)
    {
        var ec = await repo.GetByNumberAsync(cmd.EcNumber, ct);
        if (ec is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.EcNumber}' was not found.");

        ec.Approve(cmd.ApprovedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
