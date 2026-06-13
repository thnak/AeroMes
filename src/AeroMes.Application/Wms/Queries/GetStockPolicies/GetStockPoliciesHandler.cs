using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetStockPolicies;

public class GetStockPoliciesHandler(IStockPolicyRepository repo)
    : IQueryHandler<GetStockPoliciesQuery, IReadOnlyList<StockPolicyDto>>
{
    public async Task<IReadOnlyList<StockPolicyDto>> HandleAsync(
        GetStockPoliciesQuery query, CancellationToken ct)
    {
        var policies = await repo.GetAllAsync(query.IsActive, ct);
        return [.. policies.Select(p => new StockPolicyDto(
            p.PolicyId,
            p.ProductCode,
            p.LocationId,
            p.MinQty,
            p.MaxQty,
            p.SafetyStockQty,
            p.ReorderQty,
            p.LeadTimeDays,
            p.IsActive,
            p.CreatedBy,
            p.CreatedAt,
            p.UpdatedBy,
            p.UpdatedAt))];
    }
}
