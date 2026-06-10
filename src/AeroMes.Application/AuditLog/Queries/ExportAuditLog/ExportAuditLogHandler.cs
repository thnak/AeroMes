using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.AuditLog.Queries.ExportAuditLog;

public class ExportAuditLogHandler(IAuditLogRepository repo)
    : IQueryHandler<ExportAuditLogQuery, IReadOnlyList<SecurityAuditLog>>
{
    public async Task<IReadOnlyList<SecurityAuditLog>> HandleAsync(ExportAuditLogQuery q, CancellationToken ct)
        => await repo.GetForExportAsync(q.From, q.To, ct);
}
