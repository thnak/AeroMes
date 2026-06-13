using AeroMes.Application.Templates;
using AeroMes.Domain.Templates;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public sealed class DocumentTemplateRepository(AppDbContext db) : IDocumentTemplateRepository
{
    public async Task<IReadOnlyList<DocumentTemplateDto>> GetListAsync(
        string? documentType, bool? isActive, CancellationToken ct)
    {
        var q = db.DocumentTemplates.AsNoTracking();

        if (!string.IsNullOrEmpty(documentType) &&
            Enum.TryParse<DocumentType>(documentType, true, out var dt))
            q = q.Where(t => t.DocumentType == dt);

        if (isActive.HasValue)
            q = q.Where(t => t.IsActive == isActive.Value);

        return await q
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new DocumentTemplateDto(
                t.TemplateId, t.TemplateName, t.DocumentType.ToString(),
                t.OutputFormat.ToString(), t.FileId, t.IsActive, t.CreatedBy, t.CreatedAt))
            .ToListAsync(ct);
    }

    public Task<DocumentTemplate?> GetByIdAsync(int id, CancellationToken ct)
        => db.DocumentTemplates.FirstOrDefaultAsync(t => t.TemplateId == id, ct);

    public async Task<DocumentTemplateDto?> GetDtoByIdAsync(int id, CancellationToken ct)
    {
        return await db.DocumentTemplates.AsNoTracking()
            .Where(t => t.TemplateId == id)
            .Select(t => new DocumentTemplateDto(
                t.TemplateId, t.TemplateName, t.DocumentType.ToString(),
                t.OutputFormat.ToString(), t.FileId, t.IsActive, t.CreatedBy, t.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(DocumentTemplate template, CancellationToken ct)
    {
        db.DocumentTemplates.Add(template);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DocumentTemplate template, CancellationToken ct)
    {
        db.DocumentTemplates.Remove(template);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<PrintAuditLogDto>> GetPrintAuditAsync(
        string documentType, string documentId, CancellationToken ct)
    {
        return await db.PrintAuditLogs.AsNoTracking()
            .Where(l => l.DocumentType == documentType && l.DocumentId == documentId)
            .OrderByDescending(l => l.PrintedAt)
            .Select(l => new PrintAuditLogDto(
                l.LogId, l.DocumentType, l.DocumentId,
                l.TemplateId, l.TemplateName, l.OutputFormat,
                l.PrintedBy, l.PrintedAt))
            .ToListAsync(ct);
    }

    public async Task AddPrintAuditAsync(PrintAuditLog log, CancellationToken ct)
    {
        db.PrintAuditLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }
}
