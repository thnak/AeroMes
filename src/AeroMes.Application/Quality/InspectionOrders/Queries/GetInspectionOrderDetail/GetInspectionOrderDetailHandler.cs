using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Queries.GetInspectionOrderDetail;

public class GetInspectionOrderDetailHandler(IInspectionOrderRepository repo)
    : IQueryHandler<GetInspectionOrderDetailQuery, InspectionOrderDetailDto?>
{
    public async Task<InspectionOrderDetailDto?> HandleAsync(GetInspectionOrderDetailQuery q, CancellationToken ct)
    {
        var order = await repo.GetByIdWithPlanAsync(q.InspectionOrderId, ct);
        if (order is null) return null;

        return new InspectionOrderDetailDto(
            order.InspectionOrderId,
            order.OrderNo,
            order.Status,
            order.PlanId,
            order.Plan?.Code ?? string.Empty,
            order.Plan?.Name ?? string.Empty,
            order.JobId,
            order.WorkOrderId,
            order.ProductCode,
            order.LotNumber,
            order.SampleSize,
            order.TriggeredBy,
            order.InspectorCode,
            order.CreatedAt,
            order.AssignedAt,
            order.StartedAt,
            order.CompletedAt,
            order.WaivedBy,
            order.WaivedReason);
    }
}
