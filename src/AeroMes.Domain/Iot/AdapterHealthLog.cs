namespace AeroMes.Domain.Iot;

public class AdapterHealthLog
{
    public long EventId { get; private set; }
    public int AdapterId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTime EventAt { get; private set; }
    public string? Details { get; private set; }

    private AdapterHealthLog() { }

    public static AdapterHealthLog Record(int adapterId, string eventType, string? details = null) =>
        new()
        {
            AdapterId = adapterId,
            EventType = eventType,
            EventAt = DateTime.UtcNow,
            Details = details,
        };
}
