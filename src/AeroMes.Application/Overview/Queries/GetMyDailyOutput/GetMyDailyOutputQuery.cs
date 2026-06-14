using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetMyDailyOutput;

public record GetMyDailyOutputQuery(string OperatorId, DateOnly Date) : IQuery<MyDailyOutputDto>;

public class GetMyDailyOutputQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetMyDailyOutputQuery, MyDailyOutputDto>
{
    public Task<MyDailyOutputDto> HandleAsync(GetMyDailyOutputQuery query, CancellationToken ct = default)
        => repo.GetMyDailyOutputAsync(query.OperatorId, query.Date, ct);
}
