using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetCycleCountPlans;

public record GetCycleCountPlansQuery(CycleCountPlanStatus? Status)
    : IQuery<IReadOnlyList<CycleCountPlanSummaryDto>>;

public record CycleCountPlanSummaryDto(
    int PlanId,
    string PlanCode,
    CycleCountPlanType PlanType,
    DateOnly ScheduledDate,
    CycleCountPlanStatus Status,
    string? Notes,
    string? CreatedBy,
    DateTime CreatedAt);
