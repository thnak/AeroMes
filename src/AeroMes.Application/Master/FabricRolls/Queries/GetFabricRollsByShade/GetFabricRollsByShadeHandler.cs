using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollsByShade;

public class GetFabricRollsByShadeHandler(IFabricRollRepository repo)
    : IQueryHandler<GetFabricRollsByShadeQuery, IReadOnlyList<FabricRollDto>>
{
    public Task<IReadOnlyList<FabricRollDto>> HandleAsync(GetFabricRollsByShadeQuery query, CancellationToken ct)
        => repo.GetAvailableByProductAndShadeAsync(query.FabricProductCode, query.ShadeCode, ct);
}
