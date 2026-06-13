using System.Collections.Concurrent;

namespace AeroMes.Infrastructure.Iot;

public class DeadbandFilter
{
    private record State(decimal LastValue, DateTimeOffset LastTimestamp);

    private readonly ConcurrentDictionary<string, State> _cache = new();

    public bool ShouldSkip(string machineCode, string tagKey, decimal newValue,
        DateTimeOffset timestamp, double deadbandPercent, int minIntervalMs)
    {
        var key = $"{machineCode}:{tagKey}";
        if (!_cache.TryGetValue(key, out var last))
        {
            Update(key, newValue, timestamp);
            return false;
        }

        var elapsed = (timestamp - last.LastTimestamp).TotalMilliseconds;
        if (elapsed < minIntervalMs)
            return true;

        if (deadbandPercent > 0)
        {
            var delta = Math.Abs((double)(newValue - last.LastValue));
            var threshold = Math.Abs((double)last.LastValue) * deadbandPercent / 100.0;
            if (delta < threshold)
                return true;
        }

        Update(key, newValue, timestamp);
        return false;
    }

    private void Update(string key, decimal value, DateTimeOffset ts)
        => _cache[key] = new State(value, ts);
}
