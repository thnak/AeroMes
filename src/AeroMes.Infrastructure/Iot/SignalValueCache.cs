using System.Collections.Concurrent;

namespace AeroMes.Infrastructure.Iot;

public class SignalValueCache
{
    private record Entry(decimal Value, DateTimeOffset Timestamp, DateTimeOffset ConditionStart);

    private readonly ConcurrentDictionary<string, Entry> _cache = new();

    public void Update(string machineCode, string tagKey, decimal value, DateTimeOffset timestamp)
    {
        var key = $"{machineCode}:{tagKey}";
        var conditionStart = GetConditionStart(key, value);
        _cache[key] = new Entry(value, timestamp, conditionStart);
    }

    public (decimal Value, DateTimeOffset Timestamp, DateTimeOffset ConditionStart)? Get(string machineCode, string tagKey)
    {
        if (_cache.TryGetValue($"{machineCode}:{tagKey}", out var e))
            return (e.Value, e.Timestamp, e.ConditionStart);
        return null;
    }

    private DateTimeOffset GetConditionStart(string cacheKey, decimal newValue)
    {
        if (_cache.TryGetValue(cacheKey, out var existing) && existing.Value == newValue)
            return existing.ConditionStart;
        return DateTimeOffset.UtcNow;
    }
}
