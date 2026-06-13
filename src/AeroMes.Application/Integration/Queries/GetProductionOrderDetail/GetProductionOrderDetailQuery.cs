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
    byte Priority,
    string? AssignedTo,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? ProductionDeadline,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    DateTime SyncedAt,
    IReadOnlyList<WorkOrderSummaryDto> WorkOrders,
    IReadOnlyList<PoMaterialLineDto> MaterialLines,
    IReadOnlyList<PoStageDto> Stages);

public record WorkOrderSummaryDto(
    int WOID,
    string WOCode,
    int WorkCenterID,
    string? WorkCenterName,
    int TargetQuantity,
    int ActualOK,
    int ActualNG,
    string Status);

public record PoMaterialLineDto(
    int LineId,
    string MaterialCode,
    decimal StandardQty,
    decimal ActualQty,
    string Unit);

public record PoStageDto(
    int StageId,
    int SequenceNo,
    string OperationCode,
    string? WorkCenterCode,
    bool IsCompleted);
