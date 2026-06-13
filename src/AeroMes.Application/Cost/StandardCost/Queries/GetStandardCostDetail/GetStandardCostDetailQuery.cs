using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Queries.GetStandardCostDetail;

public record GetStandardCostDetailQuery(int Id) : IQuery<QueryResult<StandardCostDetailDto>>;

public record StandardCostDetailDto(
    int StdCostId,
    string ProductCode,
    int? BomHeaderId,
    int? RoutingId,
    int CostVersion,
    string Status,
    decimal TotalMaterialCost,
    decimal TotalLaborCost,
    decimal TotalMachineCost,
    decimal TotalOverheadCost,
    decimal TotalStandardCost,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    DateTime CalculatedAt,
    IReadOnlyList<StdCostMaterialLineDto> MaterialLines,
    IReadOnlyList<StdCostRoutingLineDto> RoutingLines);

public record StdCostMaterialLineDto(
    int LineId,
    string ComponentCode,
    decimal RequiredQty,
    decimal ScrapFactor,
    decimal AdjustedQty,
    decimal UnitCost,
    decimal LineTotal);

public record StdCostRoutingLineDto(
    int LineId,
    int RoutingStepId,
    string StepName,
    decimal CycleTimeSec,
    decimal LaborRateSnapshot,
    decimal MachineRateSnapshot,
    decimal LaborCostLine,
    decimal MachineCostLine);
