using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot;

/// <summary>
/// Monitors adapter liveness every 30 s.
/// Signal freshness is measured per machine (all adapters share the same MachineCode signal stream).
/// Thresholds: Connected < 60 s; Degraded 60–120 s; Disconnected > 120 s.
/// Webhook adapters use a 5-min stale window (event-driven, not polled).
/// </summary>
public sealed class AdapterWatchdogService(
    IServiceScopeFactory scopeFactory,
    ILogger<AdapterWatchdogService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval              = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ConnectedThreshold    = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan DegradedThreshold     = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan WebhookStaleDuration  = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("AdapterWatchdogService started");

        while (!ct.IsCancellationRequested)
        {
            try { await TickAsync(ct); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "AdapterWatchdogService tick failed"); }

            await Task.Delay(Interval, ct);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<Data.AppDbContext>();
        var healthRepo = sp.GetRequiredService<IAdapterHealthRepository>();
        var uow = sp.GetRequiredService<Application.Interfaces.IUnitOfWork>();

        var adapters = await db.AdapterInstances.AsNoTracking().ToListAsync(ct);
        var now = DateTime.UtcNow;

        // Pre-fetch latest signal timestamp per machine to avoid N+1 queries
        var cutoff1min = now.AddMinutes(-1);
        var machineCodeSet = adapters.Select(a => a.MachineCode).Distinct().ToList();

        var signalRate = await db.MachineSignalLogs
            .Where(l => machineCodeSet.Contains(l.MachineCode) && l.Timestamp >= cutoff1min)
            .GroupBy(l => l.MachineCode)
            .Select(g => new { MachineCode = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MachineCode, x => x.Count, ct);

        var lastSignalAt = await db.MachineSignalLogs
            .Where(l => machineCodeSet.Contains(l.MachineCode))
            .GroupBy(l => l.MachineCode)
            .Select(g => new { MachineCode = g.Key, Last = g.Max(l => l.Timestamp) })
            .ToDictionaryAsync(x => x.MachineCode, x => x.Last, ct);

        foreach (var adapter in adapters)
        {
            try
            {
                await ProcessAdapterAsync(healthRepo, adapter, now,
                    signalRate.GetValueOrDefault(adapter.MachineCode, 0),
                    lastSignalAt.TryGetValue(adapter.MachineCode, out var last) ? (DateTime?)last.UtcDateTime : null,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to process health for adapter {Id}", adapter.AdapterID);
            }
        }

        await uow.SaveChangesAsync(ct);
    }

    private static async Task ProcessAdapterAsync(
        IAdapterHealthRepository healthRepo,
        AdapterInstance adapter,
        DateTime now,
        int rate1min,
        DateTime? lastSignal,
        CancellationToken ct)
    {
        var health = await healthRepo.GetByAdapterIdAsync(adapter.AdapterID, ct);
        if (health is null)
        {
            health = AdapterHealth.Create(adapter.AdapterID, adapter.MachineCode, adapter.AdapterType.ToString());
            healthRepo.Add(health);
        }

        if (!adapter.IsEnabled)
        {
            if (health.MarkDisconnected("Adapter is disabled"))
                healthRepo.AddLog(AdapterHealthLog.Record(adapter.AdapterID, "Disabled", "Adapter marked disabled"));
            return;
        }

        if (lastSignal.HasValue)
            health.UpdateSignalMetrics(lastSignal.Value, rate1min);

        var staleAfter   = adapter.AdapterType == AdapterType.Webhook ? WebhookStaleDuration : DegradedThreshold;
        var degradeAfter = adapter.AdapterType == AdapterType.Webhook ? WebhookStaleDuration : ConnectedThreshold;

        if (!lastSignal.HasValue)
        {
            if (health.MarkUnknown())
                healthRepo.AddLog(AdapterHealthLog.Record(adapter.AdapterID, "Unknown", "No signals received yet"));
            return;
        }

        var gap = now - lastSignal.Value;

        if (gap < degradeAfter)
        {
            if (health.MarkConnected())
                healthRepo.AddLog(AdapterHealthLog.Record(adapter.AdapterID, "Connected",
                    $"Signal {(int)gap.TotalSeconds}s ago; rate {rate1min}/min"));
        }
        else if (gap < staleAfter)
        {
            if (health.MarkDegraded($"No signal for {(int)gap.TotalSeconds}s"))
                healthRepo.AddLog(AdapterHealthLog.Record(adapter.AdapterID, "Degraded",
                    $"Last signal {(int)gap.TotalSeconds}s ago"));
        }
        else
        {
            if (health.MarkDisconnected($"No signal for {(int)gap.TotalSeconds}s"))
                healthRepo.AddLog(AdapterHealthLog.Record(adapter.AdapterID, "Disconnected",
                    $"Stale — last signal {(int)gap.TotalMinutes:F1} min ago"));
        }
    }
}
