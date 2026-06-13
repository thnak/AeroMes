using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.ApplyRecallQuarantine;

public sealed class ApplyRecallQuarantineValidator : AbstractValidator<ApplyRecallQuarantineCommand>
{
    public ApplyRecallQuarantineValidator()
    {
        RuleFor(x => x.RecallID).NotEmpty();
        RuleFor(x => x.AppliedBy).NotEmpty().MaximumLength(50);
    }
}
