using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetCycleCountPlanById;

public record GetCycleCountPlanByIdQuery(int PlanId)
    : IQuery<CycleCountPlanDetailDto?>;

public record CycleCountPlanDetailDto(
    int PlanId,
    string PlanCode,
    CycleCountPlanType PlanType,
    DateOnly ScheduledDate,
    CycleCountPlanStatus Status,
    string? Notes,
    string? CreatedBy,
    DateTime CreatedAt,
    string? UpdatedBy,
    DateTime? UpdatedAt,
    IReadOnlyList<CycleCountLineDto> Lines);

public record CycleCountLineDto(
    long LineId,
    int BinId,
    int LocationId,
    string ProductCode,
    string LotNumber,
    decimal BookQty,
    decimal? CountedQty,
    decimal? VarianceQty,
    decimal? VariancePct,
    string? CountedBy,
    DateTime? CountedAt,
    CycleCountLineStatus Status);
