using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetReplenishmentAlerts;

public class GetReplenishmentAlertsHandler(IReplenishmentAlertRepository repo)
    : IQueryHandler<GetReplenishmentAlertsQuery, IReadOnlyList<ReplenishmentAlertDto>>
{
    public async Task<IReadOnlyList<ReplenishmentAlertDto>> HandleAsync(
        GetReplenishmentAlertsQuery query, CancellationToken ct)
    {
        var alerts = await repo.GetAllAsync(query.Status, ct);
        return [.. alerts.Select(a => new ReplenishmentAlertDto(
            a.AlertId,
            a.PolicyId,
            a.ProductCode,
            a.LocationId,
            a.CurrentQty,
            a.TriggeredAt,
            a.Status,
            a.AcknowledgedBy,
            a.AcknowledgedAt,
            a.LinkedPoId))];
    }
}
