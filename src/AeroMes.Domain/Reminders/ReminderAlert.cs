namespace AeroMes.Domain.Reminders;

public sealed class ReminderAlert
{
    public Guid Id { get; private set; }
    public string ReminderType { get; private set; } = "";
    public string EntityType { get; private set; } = "";
    public string EntityId { get; private set; } = "";
    public string EntityCode { get; private set; } = "";
    public string Message { get; private set; } = "";
    public bool IsRead { get; private set; }
    public string? UserId { get; private set; }
    public string Severity { get; private set; } = "Info";
    public DateTime TriggeredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private ReminderAlert() { }

    public static ReminderAlert Create(
        string reminderType,
        string entityType,
        string entityId,
        string entityCode,
        string message,
        string severity = "Warning",
        string? userId = null)
    {
        return new ReminderAlert
        {
            Id = Guid.NewGuid(),
            ReminderType = reminderType,
            EntityType = entityType,
            EntityId = entityId,
            EntityCode = entityCode,
            Message = message,
            Severity = severity,
            UserId = userId,
            IsRead = false,
            TriggeredAt = DateTime.UtcNow,
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
