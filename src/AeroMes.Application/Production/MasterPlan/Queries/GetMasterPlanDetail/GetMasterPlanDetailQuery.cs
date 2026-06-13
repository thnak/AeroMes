using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Queries.GetMasterPlanDetail;

public record MasterPlanLineDto(
    int LineId,
    string ProductCode,
    string? ProductName,
    string? UnitOfMeasure,
    decimal QuantityRequired,
    decimal PlannedQuantity,
    decimal DailyCapacity,
    decimal OpeningInventory,
    decimal ClosingInventoryForecast,
    bool IsNegativeInventory,
    string DistributionStrategy);

public record MasterPlanDetailDto(
    int MasterPlanId,
    string PlanNumber,
    string PlanName,
    string? OrganizationalUnit,
    string Granularity,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string DataSource,
    decimal WorkingHoursPerDay,
    int WorkingDaysPerWeek,
    string Status,
    string? CreatedBy,
    DateTime CreatedAt,
    string? UpdatedBy,
    DateTime? UpdatedAt,
    IReadOnlyList<MasterPlanLineDto> Lines);

public record GetMasterPlanDetailQuery(int MasterPlanId) : IQuery<QueryResult<MasterPlanDetailDto>>;
