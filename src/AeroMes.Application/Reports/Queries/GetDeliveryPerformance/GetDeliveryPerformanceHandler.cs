using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetDeliveryPerformance;

public class GetDeliveryPerformanceHandler(IProductionOrderRepository repo)
    : IQueryHandler<GetDeliveryPerformanceQuery, DeliveryPerformanceDto>
{
    public async Task<DeliveryPerformanceDto> HandleAsync(GetDeliveryPerformanceQuery q, CancellationToken ct)
    {
        var orders = await repo.GetProgressReportAsync(q.From, q.To, null, ct);

        var rows = orders.Select(o =>
        {
            string deliveryStatus;
            int? delayDays = null;

            if (o.ActualEnd.HasValue && o.PlannedEnd.HasValue)
            {
                delayDays = (int)(o.ActualEnd.Value - o.PlannedEnd.Value).TotalDays;
                deliveryStatus = delayDays <= 0 ? "OnTime" : "Late";
            }
            else
            {
                deliveryStatus = "NotCompleted";
            }

            return new DeliveryPerformanceRowDto(
                o.POID, o.POCode, o.ProductCode,
                o.TargetQty, o.ProducedOK,
                Math.Round(o.CompletionPct, 1),
                o.PlannedEnd, o.ActualEnd,
                delayDays, deliveryStatus, o.Status);
        }).ToList();

        var total = rows.Count;
        var onTime = rows.Count(r => r.DeliveryStatus == "OnTime");
        var late = rows.Count(r => r.DeliveryStatus == "Late");
        var notCompleted = rows.Count(r => r.DeliveryStatus == "NotCompleted");
        var onTimeRate = total > 0 ? Math.Round((double)onTime / total * 100, 1) : 0;

        return new DeliveryPerformanceDto(q.From, q.To, total, onTime, late, notCompleted, onTimeRate, rows);
    }
}
