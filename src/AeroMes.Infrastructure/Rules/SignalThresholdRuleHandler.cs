using AeroMes.Domain.Iot.Events;
using LiteBus.Events.Abstractions;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Rules;

/// <summary>
/// Evaluates SIGNAL_THRESHOLD rules whenever a machine signal is ingested.
/// </summary>
public sealed class SignalThresholdRuleHandler(
    RuleEvaluationService evaluationService,
    ILogger<SignalThresholdRuleHandler> logger) : IEventHandler<MachineSignalIngestedEvent>
{
    public async Task HandleAsync(MachineSignalIngestedEvent @event, CancellationToken ct)
    {
        if (@event.IsBadQuality) return;

        var ctx = new Dictionary<string, object?>
        {
            ["machineCode"] = @event.MachineCode,
            ["tagKey"] = @event.TagKey,
            ["value"] = @event.Value,
            ["unit"] = @event.Unit,
            ["timestamp"] = @event.Timestamp.ToString("O"),
        };

        try
        {
            await evaluationService.EvaluateTriggerAsync("SIGNAL_THRESHOLD", ctx, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error evaluating SIGNAL_THRESHOLD rules for {MachineCode}/{TagKey}.",
                @event.MachineCode, @event.TagKey);
        }
    }
}
