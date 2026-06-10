using AeroMes.Application.Common;
using AeroMes.Domain.Auth;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.AuditLog.Queries.QueryAuditLog;

public record QueryAuditLogQuery(
    string? ActorId, string? EventType, string? TargetType, string? Outcome,
    DateTime? From, DateTime? To, int Page, int PageSize)
    : IQuery<PagedResult<SecurityAuditLog>>;
