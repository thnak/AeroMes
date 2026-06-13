using System.Collections.Concurrent;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot;

public class SignalConfigCache(IServiceScopeFactory scopeFactory, ILogger<SignalConfigCache> logger) : ISignalConfigCache
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private ConcurrentDictionary<(int AdapterId, string SourceAddress), SignalMapping>? _cache;

    public async Task<SignalMapping?> ResolveAsync(int adapterId, string sourceAddress, CancellationToken ct = default)
    {
        var map = await EnsureCacheAsync(ct);
        return map.TryGetValue((adapterId, sourceAddress), out var mapping) && mapping.IsEnabled
            ? mapping
            : null;
    }

    public void Invalidate()
    {
        _cache = null;
        logger.LogInformation("IoT signal config cache invalidated.");
    }

    private async Task<ConcurrentDictionary<(int, string), SignalMapping>> EnsureCacheAsync(CancellationToken ct)
    {
        if (_cache is not null)
            return _cache;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cache is not null)
                return _cache;

            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ISignalMappingRepository>();
            var mappings = await repo.GetAllEnabledAsync(ct);

            var dict = new ConcurrentDictionary<(int, string), SignalMapping>();
            foreach (var m in mappings)
                dict[(m.AdapterID, m.SourceAddress)] = m;

            _cache = dict;
            logger.LogInformation("IoT signal config cache loaded: {Count} mappings.", dict.Count);
            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }
}
