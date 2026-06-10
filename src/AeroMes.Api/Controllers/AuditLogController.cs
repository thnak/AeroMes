using AeroMes.Api.Auth;
using AeroMes.Application.AuditLog.Queries.ExportAuditLog;
using AeroMes.Application.AuditLog.Queries.GetAuditLogByUser;
using AeroMes.Application.AuditLog.Queries.QueryAuditLog;
using AeroMes.Domain.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/audit-log")]
[Authorize]
public class AuditLogController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.SystemConfigure)]
    [ProducesResponseType<AeroMes.Application.Common.PagedResult<SecurityAuditLog>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Query(
        [FromQuery] string? actorId,
        [FromQuery] string? eventType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? targetType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await mediator.Send(
            new QueryAuditLogQuery(actorId, eventType, targetType, from, to, page, pageSize));
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [RequirePermission(Permissions.SystemConfigure)]
    [ProducesResponseType<IReadOnlyList<SecurityAuditLog>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUser(string userId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await mediator.Send(new GetAuditLogByUserQuery(userId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("export")]
    [RequirePermission(Permissions.ReportExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Export([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var items = await mediator.Send(new ExportAuditLogQuery(from, to));

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
