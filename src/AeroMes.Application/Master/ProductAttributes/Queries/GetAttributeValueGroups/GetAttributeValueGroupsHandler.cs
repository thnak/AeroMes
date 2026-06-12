using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetAttributeValueGroups;

public class GetAttributeValueGroupsHandler(IProductAttributeRepository repo)
    : IQueryHandler<GetAttributeValueGroupsQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> HandleAsync(GetAttributeValueGroupsQuery q, CancellationToken ct)
        => repo.GetValueGroupNamesAsync(ct);
}
