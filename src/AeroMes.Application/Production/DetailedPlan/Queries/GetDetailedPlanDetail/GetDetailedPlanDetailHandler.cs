using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Queries.GetDetailedPlanDetail;

public class GetDetailedPlanDetailHandler(
    IDetailedProductionPlanRepository repo) : IQueryHandler<GetDetailedPlanDetailQuery, QueryResult<DetailedPlanDetailDto>>
{
    public async Task<QueryResult<DetailedPlanDetailDto>> HandleAsync(
        GetDetailedPlanDetailQuery query, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdWithLinesAsync(query.DetailPlanId, ct);
        if (plan is null)
            return QueryResult<DetailedPlanDetailDto>.NotFound($"Detailed plan {query.DetailPlanId} not found.");

        var productLines = plan.ProductLines.Select(pl =>
        {
            var slots = pl.Slots.Select(s => new DppSlotDto(
                s.SlotId, s.SlotDate, s.ShiftLabel, s.AllocatedQty)).ToList();
            return new DppProductLineDto(
                pl.DppLineId, pl.ProductCode, pl.ProductName, pl.UnitOfMeasure,
                pl.TotalRequiredQty, pl.DailyCapacity,
                pl.Slots.Sum(s => s.AllocatedQty), slots);
        }).ToList();

        var dto = new DetailedPlanDetailDto(
            plan.DetailPlanId, plan.PlanNumber, plan.PlanName, plan.MasterPlanId,
            plan.OrganizationalUnit, plan.PeriodStart, plan.PeriodEnd,
            plan.Granularity.ToString(), plan.Status.ToString(),
            plan.HasProductionOrders, plan.CreatedBy, plan.CreatedAt,
            plan.UpdatedBy, plan.UpdatedAt, productLines);

        return QueryResult<DetailedPlanDetailDto>.Found(dto);
    }
}
