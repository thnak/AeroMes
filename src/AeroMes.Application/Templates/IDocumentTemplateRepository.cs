using AeroMes.Domain.Templates;

namespace AeroMes.Application.Templates;

public record DocumentTemplateDto(
    int TemplateId,
    string TemplateName,
    string DocumentType,
    string OutputFormat,
    Guid FileId,
    bool IsActive,
    string? CreatedBy,
    DateTime CreatedAt);

public record TemplateFieldItem(string FieldCode, string Description, bool IsDetail, bool IsRequired);

public record PrintAuditLogDto(
    int LogId,
    string DocumentType,
    string DocumentId,
    int? TemplateId,
    string TemplateName,
    string OutputFormat,
    string PrintedBy,
    DateTime PrintedAt);

public interface IDocumentTemplateRepository
{
    Task<IReadOnlyList<DocumentTemplateDto>> GetListAsync(string? documentType, bool? isActive, CancellationToken ct);
    Task<DocumentTemplate?> GetByIdAsync(int id, CancellationToken ct);
    Task<DocumentTemplateDto?> GetDtoByIdAsync(int id, CancellationToken ct);
    Task AddAsync(DocumentTemplate template, CancellationToken ct);
    Task DeleteAsync(DocumentTemplate template, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    Task<IReadOnlyList<PrintAuditLogDto>> GetPrintAuditAsync(string documentType, string documentId, CancellationToken ct);
    Task AddPrintAuditAsync(PrintAuditLog log, CancellationToken ct);
}
