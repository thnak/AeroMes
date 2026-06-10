using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Queries.GetAlertThresholds;

public class GetAlertThresholdsHandler(IAlertThresholdRepository repo)
    : IQueryHandler<GetAlertThresholdsQuery, IReadOnlyList<AlertThresholdDto>>
{
    public async Task<IReadOnlyList<AlertThresholdDto>> HandleAsync(GetAlertThresholdsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new AlertThresholdDto(
            x.ThresholdId, x.MetricKey, x.Scope, x.ScopeId,
            x.WarningLevel, x.CriticalLevel, x.IsActive)).ToList();
    }
}
