using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetImportTemplate;

public sealed class GetImportTemplateHandler(IImportService importService)
    : IQueryHandler<GetImportTemplateQuery, byte[]?>
{
    public async Task<byte[]?> HandleAsync(GetImportTemplateQuery query, CancellationToken ct = default)
    {
        var supported = importService.GetSupportedCategories()
            .Any(c => c.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
        if (!supported) return null;

        return await importService.GetTemplateAsync(query.Category, ct);
    }
}
