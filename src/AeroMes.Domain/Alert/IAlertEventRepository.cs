using AeroMes.Domain.Master;

namespace AeroMes.Domain.Alert;

public record AlertEventDto(
    long AlertEventId,
    int ThresholdId,
    string MetricKey,
    AlertScope Scope,
    string? ScopeId,
    string Level,
    decimal MetricValue,
    DateTimeOffset TriggeredAt,
    DateTimeOffset? AcknowledgedAt,
    string? AcknowledgedBy,
    bool IsActive,
    string? Message);

public interface IAlertEventRepository
{
    Task AddAsync(AlertEvent alertEvent, CancellationToken ct);
    Task<AlertEvent?> GetByIdAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<AlertEventDto>> GetListAsync(bool? isActive, int? thresholdId, int page, int pageSize, CancellationToken ct);
    Task<DateTimeOffset?> GetLastTriggeredAtAsync(int thresholdId, string? scopeId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
