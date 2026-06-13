using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace AeroMes.Application.Modules.Queries.GetModuleStatus;

public class GetModuleStatusHandler(IModuleStatusRepository statusRepo, IMemoryCache cache)
    : IQueryHandler<GetModuleStatusQuery, ModuleStatusResponse>
{
    internal static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(15);

    internal static readonly string[] AllModuleIds =
    [
        "production", "quality", "iot", "warehouse",
        "maintenance", "lab", "traceability", "planning",
        "master", "costing", "reports", "admin",
    ];

    public async Task<ModuleStatusResponse> HandleAsync(GetModuleStatusQuery q, CancellationToken ct)
    {
        var modules = new List<ModuleStatusDto>(AllModuleIds.Length);

        foreach (var id in AllModuleIds)
        {
            if (q.AllowedModuleIds is not null && !q.AllowedModuleIds.Contains(id))
                continue;

            var badges = await GetOrFetchBadgesAsync(id, ct);
            modules.Add(new ModuleStatusDto(id, badges));
        }

        return new ModuleStatusResponse(DateTime.UtcNow, modules, BuildAlerts(modules));
    }

    internal async Task<IReadOnlyList<BadgeDto>> GetOrFetchBadgesAsync(string moduleId, CancellationToken ct)
    {
        if (cache.TryGetValue(CacheKey(moduleId), out IReadOnlyList<BadgeDto>? hit))
            return hit!;

        var badges = await FetchBadgesAsync(moduleId, ct);
        cache.Set(CacheKey(moduleId), badges, CacheTtl);
        return badges;
    }

    private async Task<IReadOnlyList<BadgeDto>> FetchBadgesAsync(string moduleId, CancellationToken ct) =>
        moduleId switch
        {
            "production" => await BuildProductionBadgesAsync(ct),
            _ => [],
        };

    private async Task<IReadOnlyList<BadgeDto>> BuildProductionBadgesAsync(CancellationToken ct)
    {
        var activeWo = await statusRepo.CountActiveWorkOrdersAsync(ct);
        var openDowntime = await statusRepo.CountOpenDowntimeLogsAsync(ct);

        List<BadgeDto> badges = [];

        if (activeWo > 0)
            badges.Add(new BadgeDto("activeWoCount", activeWo, "info", "active WOs"));

        if (openDowntime > 0)
            badges.Add(new BadgeDto("activeDowntime", openDowntime,
                openDowntime >= 3 ? "error" : "warning", "active downtime"));

        return badges;
    }

    private static IReadOnlyList<AlertItemDto> BuildAlerts(List<ModuleStatusDto> modules)
    {
        static int SeverityRank(string s) => s switch { "error" => 2, "warning" => 1, _ => 0 };

        var alerts = modules
            .SelectMany(m => m.Badges
                .Where(b => b.Count > 0 && b.Severity != "info")
                .Select(b => new AlertItemDto(
                    b.Severity,
                    $"{b.Count} {b.Label}",
                    ModuleHref(m.Id))))
            .OrderByDescending(a => SeverityRank(a.Severity))
            .Take(5)
            .ToList();

        return alerts;
    }

    private static string ModuleHref(string moduleId) => moduleId switch
    {
        "production"    => "/production",
        "quality"       => "/quality/inspection",
        "iot"           => "/iot/monitor",
        "warehouse"     => "/warehouse/receiving",
        "maintenance"   => "/maintenance",
        "lab"           => "/lab",
        "traceability"  => "/traceability",
        _               => $"/{moduleId}",
    };

    internal static string CacheKey(string moduleId) => $"module_status_{moduleId}";
}
