using AeroMes.Application.Templates;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Documents.Queries.GetDocumentPrintTemplates;

public record AvailableTemplateItem(int? TemplateId, string TemplateName, string OutputFormat, bool IsDefault);

public record GetDocumentPrintTemplatesQuery(string DocumentType) : IQuery<IReadOnlyList<AvailableTemplateItem>>;
