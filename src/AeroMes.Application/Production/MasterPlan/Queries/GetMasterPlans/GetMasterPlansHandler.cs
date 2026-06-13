using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Queries.GetMasterPlans;

public class GetMasterPlansHandler(
    IMasterProductionPlanRepository repo) : IQueryHandler<GetMasterPlansQuery, IReadOnlyList<MasterPlanSummaryDto>>
{
    public Task<IReadOnlyList<MasterPlanSummaryDto>> HandleAsync(
        GetMasterPlansQuery query, CancellationToken ct = default)
        => repo.GetListAsync(query.OrgUnit, query.Status, query.From, query.To, ct);
}
