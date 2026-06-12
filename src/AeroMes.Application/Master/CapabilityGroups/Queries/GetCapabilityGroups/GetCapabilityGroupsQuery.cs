using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Queries.GetCapabilityGroups;

public record GetCapabilityGroupsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<CapabilityGroupDto>>;

public record CapabilityGroupDto(
    string GroupCode,
    string GroupName,
    string? Description,
    bool IsActive);
