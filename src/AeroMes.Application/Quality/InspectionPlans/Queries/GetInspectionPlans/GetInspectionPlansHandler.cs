using AeroMes.Application.Quality.InspectionPlans;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Queries.GetInspectionPlans;

public class GetInspectionPlansHandler(IInspectionPlanRepository repo)
    : IQueryHandler<GetInspectionPlansQuery, IReadOnlyList<InspectionPlanListDto>>
{
    public async Task<IReadOnlyList<InspectionPlanListDto>> HandleAsync(GetInspectionPlansQuery q, CancellationToken ct)
    {
        // GetListAsync includes Characteristics for count projection
        var plans = await repo.GetListAsync(q.RoutingStepId, q.ProductCode, q.IsActive, ct);
        return plans.Select(p => new InspectionPlanListDto(
            p.PlanId, p.Code, p.Name, p.RoutingStepId, p.ProductCode,
            p.SamplingMethod, p.SampleSize, p.AcceptNumber, p.RejectNumber,
            p.InspectionType, p.IsActive, p.Characteristics.Count)).ToList();
    }
}
