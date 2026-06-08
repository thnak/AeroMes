using Microsoft.Extensions.Caching.Memory;

namespace AeroMes.Infrastructure.Data;

public class IdempotencyStore(IMemoryCache cache)
{
    private static string Key(string idempotencyKey) => $"idempotency:{idempotencyKey}";

    public bool TryGet(string idempotencyKey, out long existingLogId)
    {
        if (cache.TryGetValue(Key(idempotencyKey), out long id))
        {
            existingLogId = id;
            return true;
        }
        existingLogId = 0;
        return false;
    }

    public void Set(string idempotencyKey, long logId)
    {
        cache.Set(Key(idempotencyKey), logId, TimeSpan.FromHours(24));
    }
}
