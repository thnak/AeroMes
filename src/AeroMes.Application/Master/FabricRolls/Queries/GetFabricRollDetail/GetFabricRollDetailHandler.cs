using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollDetail;

public class GetFabricRollDetailHandler(IFabricRollRepository repo)
    : IQueryHandler<GetFabricRollDetailQuery, FabricRollDto?>
{
    public Task<FabricRollDto?> HandleAsync(GetFabricRollDetailQuery query, CancellationToken ct)
        => repo.GetDetailAsync(query.RollID, ct);
}
