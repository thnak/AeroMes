using AeroMes.Application.Common;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Queries.GetStandardCostDetail;

public sealed class GetStandardCostDetailHandler(IStandardCostRepository repo)
    : IQueryHandler<GetStandardCostDetailQuery, QueryResult<StandardCostDetailDto>>
{
    public async Task<QueryResult<StandardCostDetailDto>> HandleAsync(
        GetStandardCostDetailQuery q, CancellationToken ct)
    {
        var h = await repo.GetByIdWithLinesAsync(q.Id, ct);
        if (h is null) return QueryResult<StandardCostDetailDto>.NotFound($"StandardCost '{q.Id}' was not found.");

        return QueryResult<StandardCostDetailDto>.Found(new StandardCostDetailDto(
            h.StdCostId, h.ProductCode, h.BomHeaderId, h.RoutingId,
            h.CostVersion, h.Status.ToString(),
            h.TotalMaterialCost, h.TotalLaborCost, h.TotalMachineCost, h.TotalOverheadCost,
            h.TotalStandardCost, h.Currency, h.EffectiveFrom, h.EffectiveTo,
            h.ApprovedBy, h.ApprovedAt, h.CalculatedAt,
            [.. h.MaterialLines.Select(m => new StdCostMaterialLineDto(
                m.LineId, m.ComponentCode, m.RequiredQty, m.ScrapFactor,
                m.AdjustedQty, m.UnitCost, m.LineTotal))],
            [.. h.RoutingLines.Select(r => new StdCostRoutingLineDto(
                r.LineId, r.RoutingStepId, r.StepName, r.CycleTimeSec,
                r.LaborRateSnapshot, r.MachineRateSnapshot,
                r.LaborCostLine, r.MachineCostLine))]));
    }
}
