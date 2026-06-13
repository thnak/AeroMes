using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Queries.GetInspectionOrders;

public class GetInspectionOrdersHandler(IInspectionOrderRepository repo)
    : IQueryHandler<GetInspectionOrdersQuery, IReadOnlyList<InspectionOrderListDto>>
{
    public async Task<IReadOnlyList<InspectionOrderListDto>> HandleAsync(GetInspectionOrdersQuery q, CancellationToken ct)
    {
        var orders = await repo.GetFilteredAsync(q.Status, q.Date, ct);
        return orders.Select(o => new InspectionOrderListDto(
            o.InspectionOrderId,
            o.OrderNo,
            o.Status,
            o.ProductCode,
            o.JobId,
            o.WorkOrderId,
            o.InspectorCode,
            o.CreatedAt,
            o.AssignedAt,
            o.CompletedAt,
            o.TriggeredBy,
            o.SampleSize,
            o.PlanId)).ToList();
    }
}
