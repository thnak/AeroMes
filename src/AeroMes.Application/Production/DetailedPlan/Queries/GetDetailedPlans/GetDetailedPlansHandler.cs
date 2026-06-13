using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Queries.GetDetailedPlans;

public class GetDetailedPlansHandler(
    IDetailedProductionPlanRepository repo) : IQueryHandler<GetDetailedPlansQuery, IReadOnlyList<DetailedPlanSummaryDto>>
{
    public Task<IReadOnlyList<DetailedPlanSummaryDto>> HandleAsync(
        GetDetailedPlansQuery query, CancellationToken ct = default)
        => repo.GetListAsync(query.MasterPlanId, query.OrgUnit, query.Status, ct);
}
