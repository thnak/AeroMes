using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroupMembers.Queries.GetMembers;

public record GetCapabilityGroupMembersQuery(string GroupCode)
    : IQuery<IReadOnlyList<CapabilityGroupMemberDto>>;

public record CapabilityGroupMemberDto(
    int MemberId,
    string GroupCode,
    CapabilityResourceType ResourceType,
    string ResourceId,
    string? AssignedBy,
    DateTime AssignedAt);
