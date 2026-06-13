using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetStockStatus;

public class GetStockStatusHandler(
    IStockPolicyRepository policyRepo,
    IInventoryStockRepository stockRepo)
    : IQueryHandler<GetStockStatusQuery, IReadOnlyList<StockStatusItemDto>>
{
    public async Task<IReadOnlyList<StockStatusItemDto>> HandleAsync(
        GetStockStatusQuery query, CancellationToken ct)
    {
        var policies = await policyRepo.GetAllAsync(isActive: true, ct);

        if (query.LocationId.HasValue)
            policies = policies.Where(p => p.LocationId == query.LocationId.Value).ToList();

        var result = new List<StockStatusItemDto>(policies.Count);
        foreach (var p in policies)
        {
            var qty = await stockRepo.GetTotalQtyAsync(p.LocationId, p.ProductCode, ct);
            var level = qty <= p.SafetyStockQty ? "CRITICAL"
                : qty <= p.MinQty ? "LOW"
                : qty > p.MaxQty ? "OVERSTOCK"
                : "OK";

            result.Add(new StockStatusItemDto(
                p.PolicyId, p.ProductCode, p.LocationId,
                qty, p.MinQty, p.MaxQty, p.SafetyStockQty, p.ReorderQty, level));
        }
        return result;
    }
}
