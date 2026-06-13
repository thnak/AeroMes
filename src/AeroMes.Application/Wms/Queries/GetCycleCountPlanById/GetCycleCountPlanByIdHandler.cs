using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetCycleCountPlanById;

public class GetCycleCountPlanByIdHandler(ICycleCountPlanRepository repo)
    : IQueryHandler<GetCycleCountPlanByIdQuery, CycleCountPlanDetailDto?>
{
    public async Task<CycleCountPlanDetailDto?> HandleAsync(
        GetCycleCountPlanByIdQuery query, CancellationToken ct)
    {
        var plan = await repo.GetByIdWithLinesAsync(query.PlanId, ct);
        if (plan is null) return null;

        return new CycleCountPlanDetailDto(
            plan.PlanId,
            plan.PlanCode,
            plan.PlanType,
            plan.ScheduledDate,
            plan.Status,
            plan.Notes,
            plan.CreatedBy,
            plan.CreatedAt,
            plan.UpdatedBy,
            plan.UpdatedAt,
            [.. plan.Lines.Select(l => new CycleCountLineDto(
                l.LineId,
                l.BinId,
                l.LocationId,
                l.ProductCode,
                l.LotNumber,
                l.BookQty,
                l.CountedQty,
                l.VarianceQty,
                l.VariancePct,
                l.CountedBy,
                l.CountedAt,
                l.Status))]);
    }
}
