using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using MediatR;

namespace AeroMes.Application.AuditLog.Queries.ExportAuditLog;

public class ExportAuditLogHandler(IAuditLogRepository repo)
    : IRequestHandler<ExportAuditLogQuery, IReadOnlyList<SecurityAuditLog>>
{
    public async Task<IReadOnlyList<SecurityAuditLog>> Handle(ExportAuditLogQuery q, CancellationToken ct)
        => await repo.GetForExportAsync(q.From, q.To, ct);
}
