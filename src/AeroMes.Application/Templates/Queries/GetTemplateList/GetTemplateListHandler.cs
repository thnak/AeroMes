using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Templates.Queries.GetTemplateList;

public sealed class GetTemplateListHandler(IDocumentTemplateRepository repo)
    : IQueryHandler<GetTemplateListQuery, IReadOnlyList<DocumentTemplateDto>>
{
    public Task<IReadOnlyList<DocumentTemplateDto>> HandleAsync(
        GetTemplateListQuery query, CancellationToken ct = default)
        => repo.GetListAsync(query.DocumentType, query.IsActive, ct);
}
