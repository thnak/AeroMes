using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetInventoryAlert;

public record GetInventoryAlertQuery : IQuery<InventoryAlertSummaryDto>;

public class GetInventoryAlertQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetInventoryAlertQuery, InventoryAlertSummaryDto>
{
    public Task<InventoryAlertSummaryDto> HandleAsync(GetInventoryAlertQuery query, CancellationToken ct = default)
        => repo.GetInventoryAlertsAsync(ct);
}
