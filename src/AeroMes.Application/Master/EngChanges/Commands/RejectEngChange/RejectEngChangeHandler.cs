using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.RejectEngChange;

public class RejectEngChangeHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow) : ICommandHandler<RejectEngChangeCommand>
{
    public async Task HandleAsync(RejectEngChangeCommand cmd, CancellationToken ct)
    {
        var ec = await repo.GetByNumberAsync(cmd.EcNumber, ct)
            ?? throw new EntityNotFoundException(nameof(EngChange), cmd.EcNumber);

        ec.Reject(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
