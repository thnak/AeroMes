using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.ReleaseHold;

public sealed class ReleaseHoldValidator : AbstractValidator<ReleaseHoldCommand>
{
    public ReleaseHoldValidator()
    {
        RuleFor(x => x.HoldID).NotEmpty();
        RuleFor(x => x.ReleasedBy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ESignatureToken).NotEmpty();
    }
}
