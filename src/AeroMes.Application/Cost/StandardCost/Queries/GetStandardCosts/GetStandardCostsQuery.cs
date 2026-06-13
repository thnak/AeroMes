using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Queries.GetStandardCosts;

public record GetStandardCostsQuery(
    string? ProductCode,
    string? Status) : IQuery<IReadOnlyList<StandardCostSummaryDto>>;
