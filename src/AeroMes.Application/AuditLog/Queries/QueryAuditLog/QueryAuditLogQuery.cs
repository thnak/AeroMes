using AeroMes.Application.Common;
using AeroMes.Domain.Auth;
using MediatR;

namespace AeroMes.Application.AuditLog.Queries.QueryAuditLog;

public record QueryAuditLogQuery(
    string? ActorId, string? EventType, string? TargetType,
    DateTime? From, DateTime? To, int Page, int PageSize)
    : IRequest<PagedResult<SecurityAuditLog>>;
