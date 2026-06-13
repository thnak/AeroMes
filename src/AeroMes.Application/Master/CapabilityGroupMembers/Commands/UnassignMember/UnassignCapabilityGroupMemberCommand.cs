using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroupMembers.Commands.UnassignMember;

public record UnassignCapabilityGroupMemberCommand(
    string GroupCode,
    int MemberId,
    string? DeletedBy) : ICommand<ValidationResult<Unit>>;
