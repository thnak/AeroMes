using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.ItemCosts.Queries.GetItemActiveCost;

public class GetItemActiveCostHandler(IItemCostHistoryRepository repository)
    : IQueryHandler<GetItemActiveCostQuery, ItemCostHistoryDto?>
{
    public async Task<ItemCostHistoryDto?> HandleAsync(GetItemActiveCostQuery query, CancellationToken ct)
    {
        var active = await repository.GetActiveAsync(query.ProductCode, query.CostType, ct);
        if (active is null) return null;
        return new ItemCostHistoryDto(
            active.CostID, active.ProductCode, active.CostType.ToString(),
            active.UnitCost, active.CostUoM, active.EffectiveFrom, active.EffectiveTo,
            active.SourceRef, active.ApprovedBy, active.CreatedAt);
    }
}
