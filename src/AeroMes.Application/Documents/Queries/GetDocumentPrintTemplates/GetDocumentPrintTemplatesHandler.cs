using AeroMes.Application.Templates;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Documents.Queries.GetDocumentPrintTemplates;

public sealed class GetDocumentPrintTemplatesHandler(IDocumentTemplateRepository repo)
    : IQueryHandler<GetDocumentPrintTemplatesQuery, IReadOnlyList<AvailableTemplateItem>>
{
    public async Task<IReadOnlyList<AvailableTemplateItem>> HandleAsync(
        GetDocumentPrintTemplatesQuery query, CancellationToken ct = default)
    {
        var custom = await repo.GetListAsync(query.DocumentType, isActive: true, ct);

        var result = new List<AvailableTemplateItem>
        {
            new(null, "Mẫu mặc định hệ thống", "Pdf", IsDefault: true)
        };

        result.AddRange(custom.Select(t =>
            new AvailableTemplateItem(t.TemplateId, t.TemplateName, t.OutputFormat, IsDefault: false)));

        return result;
    }
}
