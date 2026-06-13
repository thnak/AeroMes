namespace AeroMes.Domain.Reminders.Repositories;

public interface IReminderRepository
{
    Task<List<ReminderAlert>> GetAlertsAsync(string? userId, bool? isRead, string? reminderType, int limit, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(string? userId, CancellationToken ct = default);
    Task<ReminderAlert?> GetAlertByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> AlertExistsAsync(string reminderType, string entityId, CancellationToken ct = default);
    void AddAlert(ReminderAlert alert);

    Task MarkAllReadAsync(string? userId, CancellationToken ct = default);

    Task<List<ReminderConfiguration>> GetConfigurationsAsync(string? userId, CancellationToken ct = default);
    Task<ReminderConfiguration?> GetConfigurationAsync(string? userId, string reminderType, CancellationToken ct = default);
    void AddConfiguration(ReminderConfiguration config);
}
