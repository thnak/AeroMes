using AeroMes.Domain.Auth;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.AuditLog.Queries.GetAuditLogByUser;

public record GetAuditLogByUserQuery(string UserId, int Page, int PageSize)
    : IQuery<IReadOnlyList<SecurityAuditLog>>;
