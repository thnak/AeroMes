using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetOutputByDepartment;

public record GetOutputByDepartmentQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyList<OutputByDepartmentItem>>;

public class GetOutputByDepartmentQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetOutputByDepartmentQuery, IReadOnlyList<OutputByDepartmentItem>>
{
    public Task<IReadOnlyList<OutputByDepartmentItem>> HandleAsync(
        GetOutputByDepartmentQuery query, CancellationToken ct = default)
        => repo.GetOutputByDepartmentAsync(query.From, query.To, ct);
}
