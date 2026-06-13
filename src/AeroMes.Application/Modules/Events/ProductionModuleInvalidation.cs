using AeroMes.Application.Interfaces;
using AeroMes.Application.Modules.Queries.GetModuleStatus;
using Microsoft.Extensions.Caching.Memory;

namespace AeroMes.Application.Modules.Events;

internal static class ProductionModuleInvalidation
{
    internal static async Task InvalidateAndNotifyAsync(
        IMemoryCache cache,
        IModuleStatusRepository statusRepo,
        IModuleStatusNotifier notifier,
        CancellationToken ct)
    {
        cache.Remove(GetModuleStatusHandler.CacheKey("production"));

        var activeWo = await statusRepo.CountActiveWorkOrdersAsync(ct);
        var openDowntime = await statusRepo.CountOpenDowntimeLogsAsync(ct);

        List<BadgeDto> badges = [];

        if (activeWo > 0)
            badges.Add(new BadgeDto("activeWoCount", activeWo, "info", "active WOs"));

        if (openDowntime > 0)
            badges.Add(new BadgeDto("activeDowntime", openDowntime,
                openDowntime >= 3 ? "error" : "warning", "active downtime"));

        cache.Set(
            GetModuleStatusHandler.CacheKey("production"),
            (IReadOnlyList<BadgeDto>)badges,
            GetModuleStatusHandler.CacheTtl);

        await notifier.NotifyModuleUpdatedAsync("production", badges, ct);
    }
}
