using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using MediatR;

namespace AeroMes.Application.AuditLog.Queries.GetAuditLogByUser;

public class GetAuditLogByUserHandler(IAuditLogRepository repo)
    : IRequestHandler<GetAuditLogByUserQuery, IReadOnlyList<SecurityAuditLog>>
{
    public async Task<IReadOnlyList<SecurityAuditLog>> Handle(GetAuditLogByUserQuery q, CancellationToken ct)
        => await repo.GetByUserAsync(q.UserId, q.Page, q.PageSize, ct);
}
