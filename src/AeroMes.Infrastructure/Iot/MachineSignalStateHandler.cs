using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Events;
using AeroMes.Domain.Production;
using AeroMes.Infrastructure.Data;
using LiteBus.Events.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot;

public class MachineSignalStateHandler(
    IServiceScopeFactory scopeFactory,
    SignalValueCache cache,
    MachineStateEvaluator evaluator,
    ILogger<MachineSignalStateHandler> logger)
    : IEventHandler<MachineSignalIngestedEvent>
{
    private static readonly HashSet<string> DownStates = ["DOWN", "FAULT"];

    public async Task HandleAsync(MachineSignalIngestedEvent @event, CancellationToken ct)
    {
        cache.Update(@event.MachineCode, @event.TagKey, @event.Value, @event.Timestamp);

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventMediator = scope.ServiceProvider.GetRequiredService<IEventMediator>();

        var rules = await db.MachineStateRules
            .Where(r => r.MachineCode == @event.MachineCode && r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        var snapshot = await db.MachineStateSnapshots.FindAsync([@event.MachineCode], ct);
        if (snapshot is null)
        {
            snapshot = MachineStateSnapshot.CreateForMachine(@event.MachineCode);
            db.MachineStateSnapshots.Add(snapshot);
        }

        snapshot.UpdateLastSignal();

        var (targetState, matchedRule, matchedValue) = evaluator.Evaluate(rules, cache, @event.MachineCode);

        // Apply hysteresis
        if (matchedRule?.Hysteresis > 0 && snapshot.TriggerValue.HasValue && matchedValue.HasValue)
        {
            var diff = Math.Abs(matchedValue.Value - snapshot.TriggerValue.Value);
            if (diff < (decimal)matchedRule.Hysteresis.Value)
                targetState = snapshot.CurrentState; // stay in current state
        }

        var previousState = snapshot.CurrentState;
        var stateChangedAt = snapshot.StateChangedAt;

        if (snapshot.TransitionTo(targetState!, matchedRule?.RuleID, matchedRule?.SignalTagKey, matchedValue))
        {
            db.MachineStateHistories.Add(MachineStateHistory.Record(
                @event.MachineCode, previousState, targetState!,
                stateChangedAt, matchedRule?.RuleID, matchedRule?.SignalTagKey, matchedValue,
                isAutomatic: true));

            try
            {
                await HandleDowntimeAsync(db, @event.MachineCode, previousState, targetState!, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling auto-downtime for machine {MachineCode} transition {From} -> {To}",
                    @event.MachineCode, previousState, targetState);
            }

            await eventMediator.PublishAsync(new MachineStateChangedEvent(
                @event.MachineCode, targetState!, previousState, DateTimeOffset.UtcNow,
                matchedRule?.SignalTagKey, matchedValue), null, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task HandleDowntimeAsync(
        AppDbContext db, string machineCode, string previousState, string newState, CancellationToken ct)
    {
        var enteringDown = DownStates.Contains(newState);
        var leavingDown = DownStates.Contains(previousState) && !DownStates.Contains(newState);

        if (enteringDown)
        {
            var hasOpen = await db.DowntimeLogs
                .AnyAsync(d => d.MachineCode == machineCode && d.EndTime == null, ct);

            if (!hasOpen)
            {
                var log = DowntimeLog.Create(
                    machineCode,
                    reasonCode: newState,
                    reasonName: $"Auto-detected: {newState}",
                    startTime: DateTime.UtcNow,
                    operatorId: "SYSTEM");
                db.DowntimeLogs.Add(log);
            }
        }
        else if (leavingDown)
        {
            var openLog = await db.DowntimeLogs
                .Where(d => d.MachineCode == machineCode && d.EndTime == null)
                .OrderByDescending(d => d.StartTime)
                .FirstOrDefaultAsync(ct);

            // Only auto-close if it was auto-opened (reasonCode matches a DownState name, i.e. system-created)
            if (openLog is not null && DownStates.Contains(openLog.ReasonCode))
            {
                openLog.End(DateTime.UtcNow);
            }
        }
    }
}
