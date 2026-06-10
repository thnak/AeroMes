using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.ChangeLifecycleStatus;

public class ChangeLifecycleStatusHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<ChangeLifecycleStatusCommand>
{
    public async Task HandleAsync(ChangeLifecycleStatusCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Product", cmd.Code);
        entity.ChangeLifecycleStatus(cmd.Status, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
