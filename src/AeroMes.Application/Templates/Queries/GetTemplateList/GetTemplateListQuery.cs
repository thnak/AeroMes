using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Templates.Queries.GetTemplateList;

public record GetTemplateListQuery(string? DocumentType, bool? IsActive) : IQuery<IReadOnlyList<DocumentTemplateDto>>;
