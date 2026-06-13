using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricInventorySummary;

public record GetFabricInventorySummaryQuery(string? FabricProductCode = null)
    : IQuery<IReadOnlyList<FabricInventorySummaryDto>>;
