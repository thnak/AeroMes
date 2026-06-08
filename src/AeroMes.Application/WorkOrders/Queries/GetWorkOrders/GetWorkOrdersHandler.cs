using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using MediatR;

namespace AeroMes.Application.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersHandler(IWorkOrderRepository workOrderRepo)
    : IRequestHandler<GetWorkOrdersQuery, IReadOnlyList<WorkOrderDto>>
{
    public async Task<IReadOnlyList<WorkOrderDto>> Handle(GetWorkOrdersQuery q, CancellationToken ct)
    {
        var status = Enum.TryParse<WorkOrderStatus>(q.Status, ignoreCase: true, out var parsed)
            ? parsed
            : WorkOrderStatus.Prepared;

        var orders = await workOrderRepo.GetByStatusAsync(status, ct);

        return orders.Select(w => new WorkOrderDto(
            w.WOID,
            w.WOCode,
            w.POID,
            w.WorkCenterID,
            w.WorkCenter?.WorkCenterName,
            w.TargetQuantity.Value,
            w.ActualQtyOK.Value,
            w.ActualQtyNG.Value,
            w.Status.ToString().ToUpperInvariant()))
            .ToList();
    }
}
