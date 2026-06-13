using FluentValidation;

namespace AeroMes.Application.Master.CapabilityGroupMembers.Commands.AssignMember;

public class AssignCapabilityGroupMemberValidator : AbstractValidator<AssignCapabilityGroupMemberCommand>
{
    public AssignCapabilityGroupMemberValidator()
    {
        RuleFor(x => x.GroupCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ResourceId).NotEmpty().MaximumLength(50);
    }
}
