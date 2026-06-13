using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrderDetail;

public record GetWorkOrderDetailQuery(int Id) : IQuery<QueryResult<WorkOrderDetailDto>>;

public record WorkOrderDetailDto(
    int WOID,
    string WOCode,
    int POID,
    int WorkCenterID,
    string? WorkCenterName,
    int TargetQty,
    int ActualOK,
    int ActualNG,
    string Status,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    IReadOnlyList<JobSummaryDto> Jobs);

public record JobSummaryDto(
    long JobID,
    string MachineCode,
    string ShiftCode,
    string OperatorID,
    DateTime StartTime,
    DateTime? EndTime,
    string Status);
