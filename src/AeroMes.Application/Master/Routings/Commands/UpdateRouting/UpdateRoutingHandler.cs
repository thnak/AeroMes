using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.UpdateRouting;

public class UpdateRoutingHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateRoutingCommand>
{
    public async Task HandleAsync(UpdateRoutingCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("Routing", cmd.Id);
        entity.UpdateDetails(cmd.Name, cmd.IsDefault, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
