using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetProductionPlanGantt;

public class GetProductionPlanGanttHandler(IProductionPlanByOrderRepository repo)
    : IQueryHandler<GetProductionPlanGanttQuery, IReadOnlyList<GanttTeamDto>>
{
    public Task<IReadOnlyList<GanttTeamDto>> HandleAsync(
        GetProductionPlanGanttQuery query, CancellationToken ct)
        => repo.GetGanttDataAsync(query.From, query.To, query.TeamCode, ct);
}
