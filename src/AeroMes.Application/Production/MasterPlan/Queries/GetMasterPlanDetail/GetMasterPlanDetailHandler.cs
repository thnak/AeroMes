using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Queries.GetMasterPlanDetail;

public class GetMasterPlanDetailHandler(
    IMasterProductionPlanRepository repo) : IQueryHandler<GetMasterPlanDetailQuery, QueryResult<MasterPlanDetailDto>>
{
    public async Task<QueryResult<MasterPlanDetailDto>> HandleAsync(
        GetMasterPlanDetailQuery query, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdWithLinesAsync(query.MasterPlanId, ct);
        if (plan is null)
            return QueryResult<MasterPlanDetailDto>.NotFound($"Master plan {query.MasterPlanId} not found.");

        var lines = plan.Lines.Select(l => new MasterPlanLineDto(
            l.LineId, l.ProductCode, l.ProductName, l.UnitOfMeasure,
            l.QuantityRequired, l.PlannedQuantity, l.DailyCapacity,
            l.OpeningInventory, l.ClosingInventoryForecast,
            l.ClosingInventoryForecast < 0,
            l.DistributionStrategy.ToString()
        )).ToList();

        var dto = new MasterPlanDetailDto(
            plan.MasterPlanId, plan.PlanNumber, plan.PlanName,
            plan.OrganizationalUnit, plan.Granularity.ToString(),
            plan.PeriodStart, plan.PeriodEnd, plan.DataSource.ToString(),
            plan.WorkingHoursPerDay, plan.WorkingDaysPerWeek,
            plan.Status.ToString(), plan.CreatedBy, plan.CreatedAt,
            plan.UpdatedBy, plan.UpdatedAt, lines);

        return QueryResult<MasterPlanDetailDto>.Found(dto);
    }
}
