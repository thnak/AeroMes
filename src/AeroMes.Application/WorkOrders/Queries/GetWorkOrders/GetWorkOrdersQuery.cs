using MediatR;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrders;

public record GetWorkOrdersQuery(string? Status = null, int? WorkCenterId = null)
    : IRequest<List<WorkOrderDto>>;

public record WorkOrderDto(
    int WorkOrderId,
    string WorkOrderNo,
    string ProductCode,
    string ProductName,
    int TargetQuantity,
    int ActualQtyOK,
    int ActualQtyNG,
    string Status,
    string WorkCenterCode
);
