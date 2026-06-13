using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.ItemCosts.Queries.GetItemActiveCost;

public record GetItemActiveCostQuery(string ProductCode, ItemCostType CostType = ItemCostType.STANDARD)
    : IQuery<ItemCostHistoryDto?>;
