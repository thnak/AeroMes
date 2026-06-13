namespace AeroMes.Application.Quality.InspectionOrders;

public record InspectionOrderListDto(
    int InspectionOrderId,
    string OrderNo,
    string Status,
    string ProductCode,
    long JobId,
    long WorkOrderId,
    string? InspectorCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? CompletedAt,
    string TriggeredBy,
    int SampleSize,
    int PlanId);

public record InspectionOrderDetailDto(
    int InspectionOrderId,
    string OrderNo,
    string Status,
    int PlanId,
    string PlanCode,
    string PlanName,
    long JobId,
    long WorkOrderId,
    string ProductCode,
    string? LotNumber,
    int SampleSize,
    string TriggeredBy,
    string? InspectorCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? WaivedBy,
    string? WaivedReason);
