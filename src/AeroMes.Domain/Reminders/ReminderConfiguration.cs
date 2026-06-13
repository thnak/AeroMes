namespace AeroMes.Domain.Reminders;

public sealed class ReminderConfiguration
{
    public Guid Id { get; private set; }
    public string? UserId { get; private set; }
    public string ReminderType { get; private set; } = "";
    public bool IsEnabled { get; private set; }
    public int LeadTimeDays { get; private set; }
    public string NotificationChannel { get; private set; } = "InApp";
    public DateTime UpdatedAt { get; private set; }

    private ReminderConfiguration() { }

    public static ReminderConfiguration Create(
        string? userId,
        string reminderType,
        bool isEnabled,
        int leadTimeDays,
        string notificationChannel = "InApp")
    {
        return new ReminderConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReminderType = reminderType,
            IsEnabled = isEnabled,
            LeadTimeDays = leadTimeDays,
            NotificationChannel = notificationChannel,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Update(bool isEnabled, int leadTimeDays, string notificationChannel)
    {
        IsEnabled = isEnabled;
        LeadTimeDays = leadTimeDays;
        NotificationChannel = notificationChannel;
        UpdatedAt = DateTime.UtcNow;
    }
}
