using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.CreateRouting;

public class CreateRoutingHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateRoutingCommand, int>
{
    public async Task<int> HandleAsync(CreateRoutingCommand cmd, CancellationToken ct)
    {
        var entity = Routing.Create(cmd.Code, cmd.Name, cmd.ProductCode, cmd.IsDefault, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.RoutingID;
    }
}
