using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.ListRecalls;

public sealed class ListRecallsHandler(IRecallRepository repository)
    : IQueryHandler<ListRecallsQuery, PagedResult<RecallSummaryDto>>
{
    public async Task<PagedResult<RecallSummaryDto>> HandleAsync(
        ListRecallsQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.ListAsync(query.Status, query.Page, query.PageSize, ct);
        return new PagedResult<RecallSummaryDto>(items, total, query.Page, query.PageSize);
    }
}
