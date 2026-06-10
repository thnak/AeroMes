using AeroMes.Domain.Auth;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.AuditLog.Queries.ExportAuditLog;

public record ExportAuditLogQuery(DateTime? From, DateTime? To)
    : IQuery<IReadOnlyList<SecurityAuditLog>>;
