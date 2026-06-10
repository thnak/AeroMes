using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.AuditLog.Queries.GetAuditLogByUser;

public class GetAuditLogByUserHandler(IAuditLogRepository repo)
    : IQueryHandler<GetAuditLogByUserQuery, IReadOnlyList<SecurityAuditLog>>
{
    public async Task<IReadOnlyList<SecurityAuditLog>> HandleAsync(GetAuditLogByUserQuery q, CancellationToken ct)
        => await repo.GetByUserAsync(q.UserId, q.Page, q.PageSize, ct);
}
