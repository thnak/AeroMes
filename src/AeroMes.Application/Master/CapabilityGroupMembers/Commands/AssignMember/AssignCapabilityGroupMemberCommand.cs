using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroupMembers.Commands.AssignMember;

public record AssignCapabilityGroupMemberCommand(
    string GroupCode,
    CapabilityResourceType ResourceType,
    string ResourceId,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
