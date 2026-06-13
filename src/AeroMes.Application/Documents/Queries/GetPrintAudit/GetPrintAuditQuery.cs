using AeroMes.Application.Templates;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Documents.Queries.GetPrintAudit;

public record GetPrintAuditQuery(string DocumentType, string DocumentId)
    : IQuery<IReadOnlyList<PrintAuditLogDto>>;
