using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricInventorySummary;

public class GetFabricInventorySummaryHandler(IFabricRollRepository repo)
    : IQueryHandler<GetFabricInventorySummaryQuery, IReadOnlyList<FabricInventorySummaryDto>>
{
    public Task<IReadOnlyList<FabricInventorySummaryDto>> HandleAsync(
        GetFabricInventorySummaryQuery query, CancellationToken ct)
        => repo.GetInventorySummaryAsync(query.FabricProductCode, ct);
}
