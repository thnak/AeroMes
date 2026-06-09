namespace AeroMes.Application.Auth;

public record SecurityAuditEvent
{
    public required string EventType { get; init; }
    public string? ActorId { get; init; }
    public string ActorType { get; init; } = "USER";
    public string? ActorIp { get; init; }
    public string? ActorUserAgent { get; init; }
    public string? TargetType { get; init; }
    public string? TargetId { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string Outcome { get; init; } = "SUCCESS";
    public string? FailureReason { get; init; }
}
