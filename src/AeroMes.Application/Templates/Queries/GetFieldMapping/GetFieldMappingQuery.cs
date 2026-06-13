using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Templates.Queries.GetFieldMapping;

public record GetFieldMappingQuery(string DocumentType) : IQuery<IReadOnlyList<TemplateFieldItem>>;
