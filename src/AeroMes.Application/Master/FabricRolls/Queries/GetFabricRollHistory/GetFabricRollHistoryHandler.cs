using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollHistory;

public class GetFabricRollHistoryHandler(IFabricRollRepository repo)
    : IQueryHandler<GetFabricRollHistoryQuery, IReadOnlyList<FabricConsumptionLogDto>>
{
    public Task<IReadOnlyList<FabricConsumptionLogDto>> HandleAsync(GetFabricRollHistoryQuery query, CancellationToken ct)
        => repo.GetHistoryAsync(query.RollID, ct);
}
