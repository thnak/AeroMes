using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Events;
using AeroMes.Infrastructure.Data;
using LiteBus.Events.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot;

public class StaleSignalWatchdog(
    IServiceScopeFactory scopeFactory,
    ILogger<StaleSignalWatchdog> logger)
    : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromSeconds(120);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("StaleSignalWatchdog started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForStaleSignalsAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Error in StaleSignalWatchdog cycle.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckForStaleSignalsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventMediator = scope.ServiceProvider.GetRequiredService<IEventMediator>();

        var staleThresholdTime = DateTimeOffset.UtcNow - StaleThreshold;

        var staleSnapshots = await db.MachineStateSnapshots
            .Where(s => s.LastSignalAt < staleThresholdTime && s.CurrentState != "UNKNOWN")
            .ToListAsync(ct);

        foreach (var snapshot in staleSnapshots)
        {
            snapshot.MarkStale();

            var previousState = snapshot.CurrentState;
            var stateChangedAt = snapshot.StateChangedAt;

            if (snapshot.TransitionTo("UNKNOWN", null, null, null))
            {
                db.MachineStateHistories.Add(MachineStateHistory.Record(
                    snapshot.MachineCode, previousState, "UNKNOWN",
                    stateChangedAt, null, null, null, isAutomatic: true));

                await eventMediator.PublishAsync(new MachineStateChangedEvent(
                    snapshot.MachineCode, "UNKNOWN", previousState, DateTimeOffset.UtcNow,
                    null, null), null, ct);

                logger.LogInformation(
                    "Machine {MachineCode} transitioned to UNKNOWN (signal stale since {StaleSince}).",
                    snapshot.MachineCode, snapshot.SignalStaleSince);
            }
        }

        if (staleSnapshots.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
