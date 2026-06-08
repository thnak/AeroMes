using MediatR;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrders;

public record GetWorkOrdersQuery(string? Status) : IRequest<IReadOnlyList<WorkOrderDto>>;

public record WorkOrderDto(
    int WOID,
    string WOCode,
    int POID,
    int WorkCenterID,
    string? WorkCenterName,
    int TargetQty,
    int ActualOK,
    int ActualNG,
    string Status);
