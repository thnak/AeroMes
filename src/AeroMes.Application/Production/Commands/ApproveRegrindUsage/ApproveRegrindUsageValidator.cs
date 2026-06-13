using FluentValidation;

namespace AeroMes.Application.Production.Commands.ApproveRegrindUsage;

public class ApproveRegrindUsageValidator : AbstractValidator<ApproveRegrindUsageCommand>
{
    public ApproveRegrindUsageValidator()
    {
        RuleFor(x => x.BlendLogID).GreaterThan(0);
        RuleFor(x => x.ApprovedBy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ApprovalNotes).MaximumLength(500).When(x => x.ApprovalNotes != null);
    }
}
