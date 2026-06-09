namespace AeroMes.Domain.Auth;

public class SecurityAuditLog
{
    public long AuditId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? ActorId { get; private set; }
    public string? ActorType { get; private set; }
    public string? ActorIp { get; private set; }
    public string? ActorUserAgent { get; private set; }
    public string? TargetType { get; private set; }
    public string? TargetId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string Outcome { get; private set; } = "SUCCESS";
    public string? FailureReason { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private SecurityAuditLog() { }

    public static SecurityAuditLog Create(
        string eventType,
        string? actorId = null,
        string? actorType = null,
        string? actorIp = null,
        string? actorUserAgent = null,
        string? targetType = null,
        string? targetId = null,
        string? oldValues = null,
        string? newValues = null,
        string outcome = "SUCCESS",
        string? failureReason = null)
        => new()
        {
            EventType = eventType,
            ActorId = actorId,
            ActorType = actorType,
            ActorIp = actorIp,
            ActorUserAgent = actorUserAgent?.Length > 500 ? actorUserAgent[..500] : actorUserAgent,
            TargetType = targetType,
            TargetId = targetId,
            OldValues = oldValues,
            NewValues = newValues,
            Outcome = outcome,
            FailureReason = failureReason,
            OccurredAt = DateTime.UtcNow,
        };
}
