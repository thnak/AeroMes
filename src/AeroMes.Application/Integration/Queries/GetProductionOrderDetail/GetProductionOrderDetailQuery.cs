using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Integration.Queries.GetProductionOrderDetail;

public record GetProductionOrderDetailQuery(int Id) : IQuery<QueryResult<ProductionOrderDetailDto>>;

public record ProductionOrderDetailDto(
    int POID,
    string POCode,
    int? SOID,
    string ProductCode,
    int TargetQuantity,
    string Status,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    DateTime SyncedAt,
    IReadOnlyList<WorkOrderSummaryDto> WorkOrders);

public record WorkOrderSummaryDto(
    int WOID,
    string WOCode,
    int WorkCenterID,
    string? WorkCenterName,
    int TargetQuantity,
    int ActualOK,
    int ActualNG,
    string Status);
