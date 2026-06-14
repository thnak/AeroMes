namespace AeroMes.Domain.Alert;

public enum AlertLevel { Warning, Critical }

public class AlertEvent
{
    public long AlertEventId { get; private set; }
    public int ThresholdId { get; private set; }
    public AlertLevel Level { get; private set; }
    public string? ScopeId { get; private set; }
    public decimal MetricValue { get; private set; }
    public DateTimeOffset TriggeredAt { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public string? AcknowledgedBy { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Message { get; private set; }

    // EF navigation
    public Master.AlertThreshold? Threshold { get; private set; }

    private AlertEvent() { }

    public static AlertEvent Create(
        int thresholdId, AlertLevel level, string? scopeId, decimal metricValue, string? message = null)
    {
        return new AlertEvent
        {
            ThresholdId = thresholdId,
            Level = level,
            ScopeId = scopeId,
            MetricValue = metricValue,
            TriggeredAt = DateTimeOffset.UtcNow,
            IsActive = true,
            Message = message
        };
    }

    public void Acknowledge(string acknowledgedBy)
    {
        IsActive = false;
        AcknowledgedAt = DateTimeOffset.UtcNow;
        AcknowledgedBy = acknowledgedBy;
    }
}
