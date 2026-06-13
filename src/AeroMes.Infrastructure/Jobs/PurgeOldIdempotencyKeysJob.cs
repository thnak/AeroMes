using Hangfire;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Jobs;

public class PurgeOldIdempotencyKeysJob(ILogger<PurgeOldIdempotencyKeysJob> logger)
{
    [DisableConcurrentExecution(60)]
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        // Idempotency keys stored in IMemoryCache expire automatically at 24 h (set in IdempotencyStore.Set).
        // Production log DB entries are retained as an audit trail and are not deleted here.
        logger.LogInformation("PurgeOldIdempotencyKeysJob: memory-cache keys auto-expire; no manual cleanup required.");
        return Task.CompletedTask;
    }
}
