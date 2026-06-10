using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using MediatR;

namespace AeroMes.Application.AuditLog.Queries.QueryAuditLog;

public class QueryAuditLogHandler(IAuditLogRepository repo)
    : IRequestHandler<QueryAuditLogQuery, PagedResult<SecurityAuditLog>>
{
    public async Task<PagedResult<SecurityAuditLog>> Handle(QueryAuditLogQuery q, CancellationToken ct)
    {
        var (items, total) = await repo.QueryAsync(
            q.ActorId, q.EventType, q.TargetType,
            q.From, q.To, q.Page, q.PageSize, ct);

        return new PagedResult<SecurityAuditLog>(items, total, q.Page, q.PageSize);
    }
}
