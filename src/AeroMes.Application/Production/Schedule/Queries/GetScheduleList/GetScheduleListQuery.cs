using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Schedule.Queries.GetScheduleList;

public record GetScheduleListQuery(
    string? Status, DateTime? From, DateTime? To, int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<ScheduleListDto>>;

public class GetScheduleListQueryHandler(IProductionScheduleRepository repo)
    : IQueryHandler<GetScheduleListQuery, PagedResult<ScheduleListDto>>
{
    public async Task<PagedResult<ScheduleListDto>> HandleAsync(
        GetScheduleListQuery query, CancellationToken ct = default)
    {
        var (items, total) = await repo.GetListAsync(
            query.Status, query.From, query.To, query.Page, query.PageSize, ct);
        return new PagedResult<ScheduleListDto>(items, total, query.Page, query.PageSize);
    }
}
