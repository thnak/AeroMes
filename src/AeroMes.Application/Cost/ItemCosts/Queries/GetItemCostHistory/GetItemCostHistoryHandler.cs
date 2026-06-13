using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.ItemCosts.Queries.GetItemCostHistory;

public class GetItemCostHistoryHandler(IItemCostHistoryRepository repository)
    : IQueryHandler<GetItemCostHistoryQuery, IReadOnlyList<ItemCostHistoryDto>>
{
    public Task<IReadOnlyList<ItemCostHistoryDto>> HandleAsync(GetItemCostHistoryQuery query, CancellationToken ct)
        => repository.GetByProductAsync(query.ProductCode, ct);
}
