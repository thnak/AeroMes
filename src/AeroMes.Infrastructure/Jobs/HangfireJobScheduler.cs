using AeroMes.Application.Jobs;
using Hangfire;

namespace AeroMes.Infrastructure.Jobs;

public class HangfireJobScheduler : IJobScheduler
{
    public void RegisterRecurringJobs()
    {
        RecurringJob.AddOrUpdate<SyncErpOrdersJob>(
            "sync-erp-orders",
            j => j.ExecuteAsync(CancellationToken.None),
            "*/5 * * * *");

        RecurringJob.AddOrUpdate<GenerateMaintenanceScheduleJob>(
            "generate-maintenance-schedule",
            j => j.ExecuteAsync(CancellationToken.None),
            "30 0 * * *");

        RecurringJob.AddOrUpdate<AccumulateMachineRuntimeJob>(
            "accumulate-machine-runtime",
            j => j.ExecuteAsync(CancellationToken.None),
            "*/15 * * * *");

        RecurringJob.AddOrUpdate<AlertEvaluationTimerJob>(
            "alert-evaluation",
            j => j.ExecuteAsync(CancellationToken.None),
            "*/2 * * * *");

        RecurringJob.AddOrUpdate<PurgeOldIdempotencyKeysJob>(
            "purge-idempotency-keys",
            j => j.ExecuteAsync(CancellationToken.None),
            Cron.Daily());
    }
}
