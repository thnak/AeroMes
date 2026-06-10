using AeroMes.Domain.Auth;
using MediatR;

namespace AeroMes.Application.AuditLog.Queries.ExportAuditLog;

public record ExportAuditLogQuery(DateTime? From, DateTime? To)
    : IRequest<IReadOnlyList<SecurityAuditLog>>;
