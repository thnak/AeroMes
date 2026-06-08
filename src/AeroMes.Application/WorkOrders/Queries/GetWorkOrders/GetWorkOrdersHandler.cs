using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using MediatR;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersHandler(IWorkOrderRepository workOrderRepo)
    : IRequestHandler<GetWorkOrdersQuery, List<WorkOrderDto>>
{
    public async Task<List<WorkOrderDto>> Handle(GetWorkOrdersQuery query, CancellationToken ct)
    {
        WorkOrderStatus? status = null;
        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<WorkOrderStatus>(query.Status, ignoreCase: true, out var parsed))
            status = parsed;

        var workOrders = await workOrderRepo.GetFilteredAsync(status, query.WorkCenterId, ct);

        return workOrders.Select(wo => new WorkOrderDto(
            wo.WorkOrderID,
            wo.WorkOrderNo,
            wo.ProductCode,
            wo.ProductName,
            wo.PlannedQty.Value,
            wo.ActualQtyOK.Value,
            wo.ActualQtyNG.Value,
            wo.Status.ToString().ToUpperInvariant(),
            wo.WorkCenter?.WorkCenterCode ?? string.Empty
        )).ToList();
    }
}
