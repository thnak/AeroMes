using AeroMes.Application.Interfaces;
using AeroMes.Domain.Reminders;
using AeroMes.Domain.Reminders.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/reminders")]
[Authorize]
public sealed class RemindersController(IReminderRepository repo, IUnitOfWork uow) : ControllerBase
{
    private string? CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    // ── Alerts ────────────────────────────────────────────────────────────────

    [HttpGet("alerts")]
    [ProducesResponseType<List<ReminderAlertDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] bool? isRead,
        [FromQuery] string? type,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        if (limit is < 1 or > 200) limit = 50;
        var alerts = await repo.GetAlertsAsync(CurrentUserId, isRead, type, limit, ct);
        return Ok(alerts.Select(a => new ReminderAlertDto(
            a.Id, a.ReminderType, a.EntityType, a.EntityId, a.EntityCode,
            a.Message, a.IsRead, a.Severity, a.UserId, a.TriggeredAt, a.ReadAt)).ToList());
    }

    [HttpGet("alerts/unread-count")]
    [ProducesResponseType<UnreadCountDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var count = await repo.GetUnreadCountAsync(CurrentUserId, ct);
        return Ok(new UnreadCountDto(count));
    }

    [HttpPost("alerts/{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var alert = await repo.GetAlertByIdAsync(id, ct);
        if (alert is null) return NotFound();
        alert.MarkRead();
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("alerts/read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await repo.MarkAllReadAsync(CurrentUserId, ct);
        return NoContent();
    }

    // ── Configurations ────────────────────────────────────────────────────────

    [HttpGet("configurations")]
    [ProducesResponseType<List<ReminderConfigDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfigurations(CancellationToken ct)
    {
        var configs = await repo.GetConfigurationsAsync(CurrentUserId, ct);
        return Ok(configs.Select(c => new ReminderConfigDto(
            c.Id, c.UserId, c.ReminderType, c.IsEnabled, c.LeadTimeDays, c.NotificationChannel)));
    }

    [HttpPut("configurations/{reminderType}")]
    public async Task<IActionResult> UpsertConfiguration(
        string reminderType,
        [FromBody] UpsertReminderConfigRequest req,
        CancellationToken ct)
    {
        var existing = await repo.GetConfigurationAsync(CurrentUserId, reminderType, ct);
        if (existing is null)
        {
            var config = ReminderConfiguration.Create(
                CurrentUserId, reminderType, req.IsEnabled, req.LeadTimeDays, req.NotificationChannel);
            repo.AddConfiguration(config);
        }
        else
        {
            existing.Update(req.IsEnabled, req.LeadTimeDays, req.NotificationChannel);
        }

        await uow.SaveChangesAsync(ct);
        return NoContent();
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record ReminderAlertDto(
    Guid Id,
    string ReminderType,
    string EntityType,
    string EntityId,
    string EntityCode,
    string Message,
    bool IsRead,
    string Severity,
    string? UserId,
    DateTime TriggeredAt,
    DateTime? ReadAt);

public sealed record UnreadCountDto(int Count);

public sealed record ReminderConfigDto(
    Guid Id,
    string? UserId,
    string ReminderType,
    bool IsEnabled,
    int LeadTimeDays,
    string NotificationChannel);

public sealed record UpsertReminderConfigRequest(
    bool IsEnabled,
    int LeadTimeDays,
    string NotificationChannel = "InApp");
