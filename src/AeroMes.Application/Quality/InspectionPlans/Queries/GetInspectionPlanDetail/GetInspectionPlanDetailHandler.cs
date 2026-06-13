using AeroMes.Application.Quality.InspectionPlans;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Queries.GetInspectionPlanDetail;

public class GetInspectionPlanDetailHandler(IInspectionPlanRepository repo)
    : IQueryHandler<GetInspectionPlanDetailQuery, InspectionPlanDetailDto?>
{
    public async Task<InspectionPlanDetailDto?> HandleAsync(GetInspectionPlanDetailQuery q, CancellationToken ct)
    {
        var plan = await repo.GetByIdWithCharacteristicsAsync(q.PlanId, ct);
        if (plan is null) return null;

        var characteristics = plan.Characteristics
            .OrderBy(c => c.Sequence)
            .Select(c => new InspectionCharacteristicDto(
                c.CharId, c.PlanId, c.Sequence, c.CharName, c.MeasurementType,
                c.SpecMin, c.SpecMax, c.SpecNominal, c.Unit,
                c.AttributeSpec, c.IsRequired, c.SeverityLevel, c.DefectCodeLink, c.Notes))
            .ToList();

        return new InspectionPlanDetailDto(
            plan.PlanId, plan.Code, plan.Name, plan.RoutingStepId, plan.ProductCode,
            plan.SamplingMethod, plan.SampleSize, plan.AcceptNumber, plan.RejectNumber,
            plan.InspectionType, plan.IsActive, plan.Notes, characteristics);
    }
}
