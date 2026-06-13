using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Queries.GetDetailedPlanDetail;

public record DppSlotDto(
    int SlotId,
    DateOnly SlotDate,
    string? ShiftLabel,
    decimal AllocatedQty);

public record DppProductLineDto(
    int DppLineId,
    string ProductCode,
    string? ProductName,
    string? UnitOfMeasure,
    decimal TotalRequiredQty,
    decimal DailyCapacity,
    decimal TotalAllocatedQty,
    IReadOnlyList<DppSlotDto> Slots);

public record DetailedPlanDetailDto(
    int DetailPlanId,
    string PlanNumber,
    string PlanName,
    int MasterPlanId,
    string? OrganizationalUnit,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string Granularity,
    string Status,
    bool HasProductionOrders,
    string? CreatedBy,
    DateTime CreatedAt,
    string? UpdatedBy,
    DateTime? UpdatedAt,
    IReadOnlyList<DppProductLineDto> ProductLines);

public record GetDetailedPlanDetailQuery(int DetailPlanId) : IQuery<QueryResult<DetailedPlanDetailDto>>;
