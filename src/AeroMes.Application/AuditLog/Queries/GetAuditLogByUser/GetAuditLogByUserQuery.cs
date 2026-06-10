using AeroMes.Domain.Auth;
using MediatR;

namespace AeroMes.Application.AuditLog.Queries.GetAuditLogByUser;

public record GetAuditLogByUserQuery(string UserId, int Page, int PageSize)
    : IRequest<IReadOnlyList<SecurityAuditLog>>;
