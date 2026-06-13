using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Queries.GetInspectionRequests;

public class GetInspectionRequestsHandler(IQualityInspectionRequestRepository repository)
    : IQueryHandler<GetInspectionRequestsQuery, PagedResult<InspectionRequestDto>>
{
    public async Task<PagedResult<InspectionRequestDto>> HandleAsync(
        GetInspectionRequestsQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(
            query.Status, query.Purpose, query.From, query.To,
            query.Page, query.PageSize, ct);
        return new PagedResult<InspectionRequestDto>(items, total, query.Page, query.PageSize);
    }
}
