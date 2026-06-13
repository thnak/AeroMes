using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.ItemCosts.Queries.GetItemCostHistory;

public record GetItemCostHistoryQuery(string ProductCode) : IQuery<IReadOnlyList<ItemCostHistoryDto>>;
