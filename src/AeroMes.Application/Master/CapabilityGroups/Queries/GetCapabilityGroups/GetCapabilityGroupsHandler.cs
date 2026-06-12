using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Queries.GetCapabilityGroups;

public class GetCapabilityGroupsHandler(ICapabilityGroupRepository repo)
    : IQueryHandler<GetCapabilityGroupsQuery, IReadOnlyList<CapabilityGroupDto>>
{
    public async Task<IReadOnlyList<CapabilityGroupDto>> HandleAsync(GetCapabilityGroupsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new CapabilityGroupDto(
            x.GroupCode,
            x.GroupName,
            x.Description,
            x.IsActive)).ToList();
    }
}
