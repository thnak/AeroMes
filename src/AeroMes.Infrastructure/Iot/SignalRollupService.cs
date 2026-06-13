using AeroMes.Domain.Iot;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot;

/// <summary>
/// Background service that aggregates raw IoT signal logs into minute and hourly rollup tables,
/// and enforces retention policies by periodically deleting expired data.
///
/// NOTE: SQL Server table partitioning on MachineSignalLogs/SignalAgg_1min/SignalAgg_1hr is
/// recommended for production environments but must be applied manually by a DBA —
/// EF Core does not support partition scheme/function DDL natively.
/// </summary>
public sealed class SignalRollupService(
    IServiceScopeFactory scopeFactory,
    ILogger<SignalRollupService> logger) : BackgroundService
{
    private static readonly TimeSpan OneMinuteInterval = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan OneHourInterval = TimeSpan.FromSeconds(3600);
    private static readonly TimeSpan RetentionInterval = TimeSpan.FromHours(24);

    private DateTimeOffset _nextHourlyRollup = DateTimeOffset.UtcNow.Add(OneHourInterval);
    private DateTimeOffset _nextRetentionCleanup = DateTimeOffset.UtcNow.Add(RetentionInterval);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SignalRollupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunMinuteRollupAsync(stoppingToken);

                var now = DateTimeOffset.UtcNow;

                if (now >= _nextHourlyRollup)
                {
                    await RunHourlyRollupAsync(stoppingToken);
                    _nextHourlyRollup = now.Add(OneHourInterval);
                }

                if (now >= _nextRetentionCleanup)
                {
                    await RunRetentionCleanupAsync(stoppingToken);
                    _nextRetentionCleanup = now.Add(RetentionInterval);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SignalRollupService encountered an error.");
            }

            await Task.Delay(OneMinuteInterval, stoppingToken);
        }

        logger.LogInformation("SignalRollupService stopped.");
    }

    private async Task RunMinuteRollupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var oneMinuteAgo = DateTimeOffset.UtcNow.AddMinutes(-1);
        var bucket = new DateTimeOffset(oneMinuteAgo.Year, oneMinuteAgo.Month, oneMinuteAgo.Day,
            oneMinuteAgo.Hour, oneMinuteAgo.Minute, 0, TimeSpan.Zero);

        var groups = await db.MachineSignalLogs
            .Where(l => l.Timestamp >= bucket && l.Timestamp < bucket.AddMinutes(1))
            .GroupBy(l => new { l.MachineCode, l.TagKey })
            .Select(g => new
            {
                g.Key.MachineCode, g.Key.TagKey,
                Count = g.Count(),
                Sum = g.Sum(x => x.Value),
                Min = g.Min(x => x.Value),
                Max = g.Max(x => x.Value),
                First = g.OrderBy(x => x.Timestamp).Select(x => x.Value).First(),
                Last = g.OrderByDescending(x => x.Timestamp).Select(x => x.Value).First(),
            })
            .ToListAsync(ct);

        foreach (var g in groups)
        {
            var existing = await db.SignalAgg1mins.FirstOrDefaultAsync(
                a => a.MachineCode == g.MachineCode && a.TagKey == g.TagKey && a.BucketAt == bucket, ct);
            if (existing is null)
                db.SignalAgg1mins.Add(SignalAgg1min.Create(g.MachineCode, g.TagKey, bucket,
                    g.Count, g.Sum, g.Min, g.Max, g.First, g.Last));
            // else skip — idempotent, don't overwrite existing bucket
        }

        if (groups.Count > 0)
            await db.SaveChangesAsync(ct);

        logger.LogDebug("1-min rollup for bucket {Bucket}: {Count} groups processed.", bucket, groups.Count);
    }

    private async Task RunHourlyRollupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var oneHourAgo = DateTimeOffset.UtcNow.AddHours(-1);
        var bucket = new DateTimeOffset(oneHourAgo.Year, oneHourAgo.Month, oneHourAgo.Day,
            oneHourAgo.Hour, 0, 0, TimeSpan.Zero);

        var groups = await db.SignalAgg1mins
            .Where(a => a.BucketAt >= bucket && a.BucketAt < bucket.AddHours(1))
            .GroupBy(a => new { a.MachineCode, a.TagKey })
            .Select(g => new
            {
                g.Key.MachineCode, g.Key.TagKey,
                Count = g.Sum(x => x.SampleCount),
                Sum = g.Sum(x => x.SumValue),
                Min = g.Min(x => x.MinValue),
                Max = g.Max(x => x.MaxValue),
                First = g.OrderBy(x => x.BucketAt).Select(x => x.FirstValue).First(),
                Last = g.OrderByDescending(x => x.BucketAt).Select(x => x.LastValue).First(),
            })
            .ToListAsync(ct);

        foreach (var g in groups)
        {
            var existing = await db.SignalAgg1hrs.FirstOrDefaultAsync(
                a => a.MachineCode == g.MachineCode && a.TagKey == g.TagKey && a.BucketAt == bucket, ct);
            if (existing is null)
                db.SignalAgg1hrs.Add(SignalAgg1hr.Create(g.MachineCode, g.TagKey, bucket,
                    g.Count, g.Sum, g.Min, g.Max, g.First, g.Last));
            // else skip — idempotent, don't overwrite existing bucket
        }

        if (groups.Count > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation("1-hr rollup for bucket {Bucket}: {Count} groups processed.", bucket, groups.Count);
    }

    private async Task RunRetentionCleanupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var policy = await db.RetentionPolicies.FirstOrDefaultAsync(p => p.Scope == "GLOBAL", ct)
            ?? RetentionPolicy.CreateGlobal(30, 90, 730);

        var rawCutoff = DateTimeOffset.UtcNow.AddDays(-policy.RawRetentionDays);
        var rawDeleted = await db.MachineSignalLogs
            .Where(l => l.Timestamp < rawCutoff)
            .ExecuteDeleteAsync(ct);

        var agg1minCutoff = DateTimeOffset.UtcNow.AddDays(-policy.Agg1minRetentionDays);
        var agg1minDeleted = await db.SignalAgg1mins
            .Where(a => a.BucketAt < agg1minCutoff)
            .ExecuteDeleteAsync(ct);

        var agg1hrCutoff = DateTimeOffset.UtcNow.AddDays(-policy.Agg1hrRetentionDays);
        var agg1hrDeleted = await db.SignalAgg1hrs
            .Where(a => a.BucketAt < agg1hrCutoff)
            .ExecuteDeleteAsync(ct);

        logger.LogInformation(
            "Retention cleanup: raw={Raw}, agg1min={Agg1min}, agg1hr={Agg1hr} rows deleted.",
            rawDeleted, agg1minDeleted, agg1hrDeleted);
    }
}
