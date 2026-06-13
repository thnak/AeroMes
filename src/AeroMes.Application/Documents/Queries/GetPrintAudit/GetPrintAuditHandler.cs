using AeroMes.Application.Templates;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Documents.Queries.GetPrintAudit;

public sealed class GetPrintAuditHandler(IDocumentTemplateRepository repo)
    : IQueryHandler<GetPrintAuditQuery, IReadOnlyList<PrintAuditLogDto>>
{
    public Task<IReadOnlyList<PrintAuditLogDto>> HandleAsync(
        GetPrintAuditQuery query, CancellationToken ct = default)
        => repo.GetPrintAuditAsync(query.DocumentType, query.DocumentId, ct);
}
