using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Templates.Queries.GetTemplateDetail;

public sealed class GetTemplateDetailHandler(IDocumentTemplateRepository repo)
    : IQueryHandler<GetTemplateDetailQuery, DocumentTemplateDto?>
{
    public Task<DocumentTemplateDto?> HandleAsync(
        GetTemplateDetailQuery query, CancellationToken ct = default)
        => repo.GetDtoByIdAsync(query.TemplateId, ct);
}
