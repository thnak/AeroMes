using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRouting;

public class DeleteRoutingHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteRoutingCommand>
{
    public async Task Handle(DeleteRoutingCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("Routing", cmd.Id);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
