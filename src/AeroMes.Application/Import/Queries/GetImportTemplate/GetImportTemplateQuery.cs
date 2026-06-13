using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetImportTemplate;

public record GetImportTemplateQuery(string Category) : IQuery<byte[]?>;
