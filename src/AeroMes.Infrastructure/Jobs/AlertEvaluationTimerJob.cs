using Hangfire;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Jobs;

public class AlertEvaluationTimerJob(ILogger<AlertEvaluationTimerJob> logger)
{
    [DisableConcurrentExecution(30)]
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        logger.LogDebug("AlertEvaluationTimerJob: evaluating time-based alert conditions.");
        // Full implementation will be added in issue #11 alongside downtime SLA alerts.
        return Task.CompletedTask;
    }
}
