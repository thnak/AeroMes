using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.ApproveEngChange;

public class ApproveEngChangeHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow) : ICommandHandler<ApproveEngChangeCommand>
{
    public async Task HandleAsync(ApproveEngChangeCommand cmd, CancellationToken ct)
    {
        var ec = await repo.GetByNumberAsync(cmd.EcNumber, ct)
            ?? throw new EntityNotFoundException(nameof(EngChange), cmd.EcNumber);

        ec.Approve(cmd.ApprovedBy);
        await uow.SaveChangesAsync(ct);
    }
}
