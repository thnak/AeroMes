using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Queries.GetAlertThresholds;

public class GetAlertThresholdsHandler(
    IAlertThresholdRepository repo,
    IMachineRepository machineRepo,
    IWorkCenterRepository workCenterRepo)
    : IQueryHandler<GetAlertThresholdsQuery, IReadOnlyList<AlertThresholdDto>>
{
    public async Task<IReadOnlyList<AlertThresholdDto>> HandleAsync(GetAlertThresholdsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);

        // Collect unique ScopeIds per resource type to batch resolve them
        var machineIds = items
            .Where(x => x.Scope == AlertScope.Machine && x.ScopeId != null)
            .Select(x => x.ScopeId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var wcIds = items
            .Where(x => x.Scope == AlertScope.WorkCenter && x.ScopeId != null)
            .Select(x => x.ScopeId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var resolvedMachines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in machineIds)
            if (await machineRepo.ExistsAsync(id, ct))
                resolvedMachines.Add(id);

        var resolvedWorkCenters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in wcIds)
            if (await workCenterRepo.CodeExistsAsync(id, ct))
                resolvedWorkCenters.Add(id);

        return items.Select(x =>
        {
            var isOrphaned = x.Scope switch
            {
                AlertScope.Machine => x.ScopeId != null && !resolvedMachines.Contains(x.ScopeId),
                AlertScope.WorkCenter => x.ScopeId != null && !resolvedWorkCenters.Contains(x.ScopeId),
                _ => false
            };
            return new AlertThresholdDto(
                x.ThresholdId, x.MetricKey, x.Scope, x.ScopeId,
                x.WarningLevel, x.CriticalLevel, x.IsActive, isOrphaned,
                x.CooldownMinutes, x.EmailEnabled);
        }).ToList();
    }
}
