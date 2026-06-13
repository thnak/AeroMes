using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroupMembers.Queries.GetMembers;

public class GetCapabilityGroupMembersHandler(ICapabilityGroupMemberRepository memberRepo)
    : IQueryHandler<GetCapabilityGroupMembersQuery, IReadOnlyList<CapabilityGroupMemberDto>>
{
    public async Task<IReadOnlyList<CapabilityGroupMemberDto>> HandleAsync(
        GetCapabilityGroupMembersQuery q, CancellationToken ct)
    {
        var members = await memberRepo.GetByGroupAsync(q.GroupCode, ct);
        return members.Select(x => new CapabilityGroupMemberDto(
            x.MemberId, x.GroupCode, x.ResourceType, x.ResourceId,
            x.CreatedBy, x.CreatedAt)).ToList();
    }
}
