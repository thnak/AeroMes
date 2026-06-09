using AeroMes.Api.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/audit-log")]
[Authorize]
public class AuditLogController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.SystemConfigure)]
    public async Task<IActionResult> Query(
        [FromQuery] string? actorId,
        [FromQuery] string? eventType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? targetType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = db.SecurityAuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(actorId))
            query = query.Where(x => x.ActorId == actorId);

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(x => x.EventType == eventType);

        if (!string.IsNullOrWhiteSpace(targetType))
            query = query.Where(x => x.TargetType == targetType);

        if (from.HasValue) query = query.Where(x => x.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.OccurredAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("user/{userId}")]
    [RequirePermission(Permissions.SystemConfigure)]
    public async Task<IActionResult> GetByUser(string userId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var items = await db.SecurityAuditLogs
            .AsNoTracking()
            .Where(x => x.ActorId == userId || x.TargetId == userId)
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("export")]
    [RequirePermission(Permissions.ReportExport)]
    public async Task<IActionResult> Export([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = db.SecurityAuditLogs.AsNoTracking();
        if (from.HasValue) query = query.Where(x => x.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.OccurredAt <= to.Value);

        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(10_000) // safety cap
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("AuditId,EventType,ActorId,ActorType,ActorIp,TargetType,TargetId,Outcome,FailureReason,OccurredAt");

        foreach (var r in items)
        {
            csv.AppendLine(string.Join(",",
                r.AuditId, Escape(r.EventType), Escape(r.ActorId), Escape(r.ActorType),
                Escape(r.ActorIp), Escape(r.TargetType), Escape(r.TargetId),
                Escape(r.Outcome), Escape(r.FailureReason),
                r.OccurredAt.ToString("o")));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv",
            $"audit-log-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Escape(string? value)
        => value is null ? string.Empty : $"\"{value.Replace("\"", "\"\"")}\"";
}
