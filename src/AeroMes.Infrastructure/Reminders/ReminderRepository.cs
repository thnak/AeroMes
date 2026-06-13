using AeroMes.Domain.Reminders;
using AeroMes.Domain.Reminders.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Reminders;

public sealed class ReminderRepository(AppDbContext db) : IReminderRepository
{
    public Task<List<ReminderAlert>> GetAlertsAsync(string? userId, bool? isRead, string? reminderType, int limit, CancellationToken ct = default)
    {
        var q = db.ReminderAlerts.AsNoTracking().AsQueryable();
        if (userId != null) q = q.Where(a => a.UserId == null || a.UserId == userId);
        if (isRead.HasValue) q = q.Where(a => a.IsRead == isRead.Value);
        if (!string.IsNullOrEmpty(reminderType)) q = q.Where(a => a.ReminderType == reminderType);
        return q.OrderByDescending(a => a.TriggeredAt).Take(limit).ToListAsync(ct);
    }

    public Task<int> GetUnreadCountAsync(string? userId, CancellationToken ct = default)
    {
        var q = db.ReminderAlerts.AsNoTracking().Where(a => !a.IsRead);
        if (userId != null) q = q.Where(a => a.UserId == null || a.UserId == userId);
        return q.CountAsync(ct);
    }

    public Task<ReminderAlert?> GetAlertByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ReminderAlerts.FindAsync([id], ct).AsTask();

    public Task<bool> AlertExistsAsync(string reminderType, string entityId, CancellationToken ct = default) =>
        db.ReminderAlerts.AnyAsync(a => a.ReminderType == reminderType && a.EntityId == entityId && !a.IsRead, ct);

    public void AddAlert(ReminderAlert alert) => db.ReminderAlerts.Add(alert);

    public Task MarkAllReadAsync(string? userId, CancellationToken ct = default)
    {
        if (userId != null)
            return db.ReminderAlerts.Where(a => !a.IsRead && (a.UserId == null || a.UserId == userId))
                     .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true).SetProperty(a => a.ReadAt, DateTime.UtcNow), ct);
        return db.ReminderAlerts.Where(a => !a.IsRead)
                 .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true).SetProperty(a => a.ReadAt, DateTime.UtcNow), ct);
    }

    public Task<List<ReminderConfiguration>> GetConfigurationsAsync(string? userId, CancellationToken ct = default)
    {
        var q = db.ReminderConfigurations.AsNoTracking().AsQueryable();
        if (userId != null) q = q.Where(c => c.UserId == null || c.UserId == userId);
        return q.ToListAsync(ct);
    }

    public Task<ReminderConfiguration?> GetConfigurationAsync(string? userId, string reminderType, CancellationToken ct = default) =>
        db.ReminderConfigurations.FirstOrDefaultAsync(c => c.UserId == userId && c.ReminderType == reminderType, ct);

    public void AddConfiguration(ReminderConfiguration config) => db.ReminderConfigurations.Add(config);
}
