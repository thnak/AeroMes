using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetPackagingBoms;

public class GetPackagingBomsHandler(IPackagingRepository repo)
    : IQueryHandler<GetPackagingBomsQuery, IReadOnlyList<PackagingBomDto>>
{
    public Task<IReadOnlyList<PackagingBomDto>> HandleAsync(GetPackagingBomsQuery query, CancellationToken ct)
        => repo.GetBomsAsync(query.ProductCode, ct);
}
