using Hangfire;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Jobs;

public class GenerateMaintenanceScheduleJob(ILogger<GenerateMaintenanceScheduleJob> logger)
{
    [DisableConcurrentExecution(120)]
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        logger.LogInformation("GenerateMaintenanceScheduleJob: checking for due PM plans.");
        // Full implementation wired to maintenance plan evaluation will be added in issue #9.
        return Task.CompletedTask;
    }
}
