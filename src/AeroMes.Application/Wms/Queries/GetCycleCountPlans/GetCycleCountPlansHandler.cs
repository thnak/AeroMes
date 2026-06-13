using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetCycleCountPlans;

public class GetCycleCountPlansHandler(ICycleCountPlanRepository repo)
    : IQueryHandler<GetCycleCountPlansQuery, IReadOnlyList<CycleCountPlanSummaryDto>>
{
    public async Task<IReadOnlyList<CycleCountPlanSummaryDto>> HandleAsync(
        GetCycleCountPlansQuery query, CancellationToken ct)
    {
        var plans = await repo.GetAllAsync(query.Status, ct);
        return [.. plans.Select(p => new CycleCountPlanSummaryDto(
            p.PlanId,
            p.PlanCode,
            p.PlanType,
            p.ScheduledDate,
            p.Status,
            p.Notes,
            p.CreatedBy,
            p.CreatedAt))];
    }
}
