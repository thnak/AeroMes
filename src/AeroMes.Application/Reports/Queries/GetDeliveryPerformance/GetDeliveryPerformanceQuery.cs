using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetDeliveryPerformance;

public record GetDeliveryPerformanceQuery(DateTime From, DateTime To) : IQuery<DeliveryPerformanceDto>;

public record DeliveryPerformanceDto(
    DateTime From,
    DateTime To,
    int TotalOrders,
    int OnTime,
    int Late,
    int NotCompleted,
    double OnTimeRatePct,
    IReadOnlyList<DeliveryPerformanceRowDto> Rows);

public record DeliveryPerformanceRowDto(
    int POID,
    string POCode,
    string ProductCode,
    int TargetQty,
    int ProducedOK,
    double CompletionPct,
    DateTime? PlannedEnd,
    DateTime? ActualEnd,
    int? DelayDays,
    string DeliveryStatus,
    string Status);
