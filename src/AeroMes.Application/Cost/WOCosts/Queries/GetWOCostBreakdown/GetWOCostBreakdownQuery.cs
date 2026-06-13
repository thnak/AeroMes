using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Queries.GetWOCostBreakdown;

public record GetWOCostBreakdownQuery(int WOID, string? CostLineType) : IQuery<WOCostBreakdownResult?>;

public record WOCostBreakdownResult(
    WOCostSummaryDto? Summary,
    IReadOnlyList<WOMaterialCostLineDto>? MaterialLines,
    IReadOnlyList<WOLaborCostLineDto>? LaborLines,
    IReadOnlyList<WOMachineCostLineDto>? MachineLines);
