using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.CreateRouting;

public class CreateRoutingHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateRoutingCommand, int>
{
    public async Task<int> Handle(CreateRoutingCommand cmd, CancellationToken ct)
    {
        var entity = Routing.Create(cmd.Code, cmd.Name, cmd.ProductCode, cmd.IsDefault, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.RoutingID;
    }
}
