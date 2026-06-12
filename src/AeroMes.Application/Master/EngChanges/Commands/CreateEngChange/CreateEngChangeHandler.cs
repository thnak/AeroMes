using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.CreateEngChange;

public class CreateEngChangeHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateEngChangeCommand, string>
{
    public async Task<string> HandleAsync(CreateEngChangeCommand cmd, CancellationToken ct)
    {
        var ec = EngChange.Create(
            cmd.EcNumber, cmd.EcType, cmd.Title, cmd.Description,
            cmd.Reason, cmd.Priority, cmd.TargetDate,
            cmd.AffectedProducts, null, cmd.RequestedBy);
        await repo.AddAsync(ec, ct);
        await uow.SaveChangesAsync(ct);
        return ec.EcNumber;
    }
}
