using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Routings.Queries.GetRoutings;

public class GetRoutingsHandler(IRoutingRepository repo)
    : IRequestHandler<GetRoutingsQuery, IReadOnlyList<RoutingDto>>
{
    public async Task<IReadOnlyList<RoutingDto>> Handle(GetRoutingsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new RoutingDto(
            x.RoutingID,
            x.RoutingCode,
            x.RoutingName,
            x.ProductCode,
            x.IsDefault,
            x.IsActive)).ToList();
    }
}
