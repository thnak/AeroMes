using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Queries.GetWOCostBreakdown;

public class GetWOCostBreakdownHandler(IWOCostRepository repository)
    : IQueryHandler<GetWOCostBreakdownQuery, WOCostBreakdownResult?>
{
    public async Task<WOCostBreakdownResult?> HandleAsync(
        GetWOCostBreakdownQuery query, CancellationToken ct)
    {
        var summary = await repository.GetSummaryDtoAsync(query.WOID, ct);
        if (summary is null) return null;

        var type = query.CostLineType?.ToLowerInvariant();
        var materialLines = (type is null or "material")
            ? await repository.GetMaterialLinesAsync(query.WOID, ct) : null;
        var laborLines = (type is null or "labor")
            ? await repository.GetLaborLinesAsync(query.WOID, ct) : null;
        var machineLines = (type is null or "machine")
            ? await repository.GetMachineLinesAsync(query.WOID, ct) : null;

        return new WOCostBreakdownResult(summary, materialLines, laborLines, machineLines);
    }
}
