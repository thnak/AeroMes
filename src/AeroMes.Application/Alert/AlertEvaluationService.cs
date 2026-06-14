using AeroMes.Domain.Alert;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Application.Interfaces;

namespace AeroMes.Application.Alert;

public class AlertEvaluationService(
    IAlertThresholdRepository thresholdRepo,
    IAlertEventRepository alertEventRepo,
    IAlertNotifier notifier)
{
    public async Task EvaluateAsync(
        string metricKey, string? scopeId, decimal currentValue, CancellationToken ct)
    {
        var thresholds = await thresholdRepo.GetAllAsync(activeOnly: true, ct);
        var matching = thresholds.Where(t =>
            t.MetricKey.Equals(metricKey, StringComparison.OrdinalIgnoreCase) &&
            (t.ScopeId == null || t.ScopeId.Equals(scopeId, StringComparison.OrdinalIgnoreCase)));

        foreach (var threshold in matching)
        {
            var level = DetermineLevel(threshold, currentValue);
            if (level is null) continue;

            // Debounce check
            var lastTriggered = await alertEventRepo.GetLastTriggeredAtAsync(threshold.ThresholdId, scopeId, ct);
            if (lastTriggered.HasValue &&
                (DateTimeOffset.UtcNow - lastTriggered.Value).TotalMinutes < threshold.CooldownMinutes)
                continue;

            var msg = $"{metricKey} = {currentValue:F2} (threshold: Warning={threshold.WarningLevel}, Critical={threshold.CriticalLevel})";
            var alertEvent = AlertEvent.Create(threshold.ThresholdId, level.Value, scopeId, currentValue, msg);
            await alertEventRepo.AddAsync(alertEvent, ct);
            await alertEventRepo.SaveChangesAsync(ct);

            var dto = new AlertEventDto(
                alertEvent.AlertEventId, threshold.ThresholdId, metricKey,
                threshold.Scope, scopeId, level.Value.ToString(),
                currentValue, alertEvent.TriggeredAt, null, null, true, msg);

            await notifier.PushAsync(dto, ct);
        }
    }

    private static AlertLevel? DetermineLevel(AlertThreshold threshold, decimal value)
    {
        // For downward-bounded metrics (OEE, quality rate): alert when BELOW threshold
        // For upward-bounded metrics (downtime, rejection): alert when ABOVE threshold
        var isDownwardMetric = IsDownwardBounded(threshold.MetricKey);

        if (isDownwardMetric)
        {
            if (value <= threshold.CriticalLevel) return AlertLevel.Critical;
            if (value <= threshold.WarningLevel) return AlertLevel.Warning;
        }
        else
        {
            if (value >= threshold.CriticalLevel) return AlertLevel.Critical;
            if (value >= threshold.WarningLevel) return AlertLevel.Warning;
        }
        return null;
    }

    private static bool IsDownwardBounded(string metricKey) => metricKey switch
    {
        "OEE" or "AVAILABILITY" or "PERFORMANCE" or "QUALITY_RATE" => true,
        _ => false
    };
}
