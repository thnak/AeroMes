using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public enum AlertScope { Factory, WorkCenter, Machine }

public class AlertThreshold : AuditableEntity
{
    public int ThresholdId { get; private set; }
    public string MetricKey { get; private set; } = string.Empty;
    public AlertScope Scope { get; private set; }
    public string? ScopeId { get; private set; }
    public decimal WarningLevel { get; private set; }
    public decimal CriticalLevel { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int CooldownMinutes { get; private set; } = 30;
    public bool EmailEnabled { get; private set; }

    private AlertThreshold() { }

    public static AlertThreshold Create(
        string metricKey, AlertScope scope,
        decimal warningLevel, decimal criticalLevel,
        string? scopeId = null,
        string? createdBy = null)
    {
        return new AlertThreshold
        {
            MetricKey = metricKey.Trim().ToUpperInvariant(),
            Scope = scope,
            ScopeId = scopeId,
            WarningLevel = warningLevel,
            CriticalLevel = criticalLevel,
            IsActive = true,
            CooldownMinutes = 30,
            EmailEnabled = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string metricKey, AlertScope scope, string? scopeId,
        decimal warningLevel, decimal criticalLevel, bool isActive,
        int cooldownMinutes, bool emailEnabled,
        string? updatedBy)
    {
        MetricKey = metricKey.Trim().ToUpperInvariant();
        Scope = scope;
        ScopeId = scopeId;
        WarningLevel = warningLevel;
        CriticalLevel = criticalLevel;
        IsActive = isActive;
        CooldownMinutes = cooldownMinutes > 0 ? cooldownMinutes : 30;
        EmailEnabled = emailEnabled;
        Touch(updatedBy);
    }
}
