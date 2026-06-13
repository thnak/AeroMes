using Hangfire;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Jobs;

public class AccumulateMachineRuntimeJob(ILogger<AccumulateMachineRuntimeJob> logger)
{
    [DisableConcurrentExecution(120)]
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        logger.LogInformation("AccumulateMachineRuntimeJob: accumulating machine runtime from signal data.");
        // Full implementation will be added in issue #9 alongside PM trigger logic.
        return Task.CompletedTask;
    }
}
