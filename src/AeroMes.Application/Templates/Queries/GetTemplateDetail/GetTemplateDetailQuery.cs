using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Templates.Queries.GetTemplateDetail;

public record GetTemplateDetailQuery(int TemplateId) : IQuery<DocumentTemplateDto?>;
