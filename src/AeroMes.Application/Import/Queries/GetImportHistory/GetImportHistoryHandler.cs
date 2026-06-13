using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetImportHistory;

public sealed class GetImportHistoryHandler(IImportRepository repo)
    : IQueryHandler<GetImportHistoryQuery, PagedResult<ImportJobSummaryDto>>
{
    public async Task<PagedResult<ImportJobSummaryDto>> HandleAsync(
        GetImportHistoryQuery query, CancellationToken ct = default)
    {
        var items = await repo.GetHistoryAsync(query.Page, query.PageSize, ct);
        return new PagedResult<ImportJobSummaryDto>(items, items.Count, query.Page, query.PageSize);
    }
}
