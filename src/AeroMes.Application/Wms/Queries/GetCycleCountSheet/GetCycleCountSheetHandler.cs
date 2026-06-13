using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetCycleCountSheet;

public class GetCycleCountSheetHandler(ICycleCountPlanRepository repo)
    : IQueryHandler<GetCycleCountSheetQuery, IReadOnlyList<CycleCountSheetLineDto>?>
{
    public async Task<IReadOnlyList<CycleCountSheetLineDto>?> HandleAsync(
        GetCycleCountSheetQuery query, CancellationToken ct)
    {
        var plan = await repo.GetByIdWithLinesAsync(query.PlanId, ct);
        if (plan is null) return null;

        return [.. plan.Lines
            .OrderBy(l => l.BinId).ThenBy(l => l.ProductCode)
            .Select(l => new CycleCountSheetLineDto(
                l.LineId,
                l.BinId,
                l.ProductCode,
                l.LotNumber,
                l.CountedQty,
                l.Status))];
    }
}
