using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.CloseRecall;

public sealed class CloseRecallValidator : AbstractValidator<CloseRecallCommand>
{
    public CloseRecallValidator()
    {
        RuleFor(x => x.RecallID).NotEmpty();
        RuleFor(x => x.ClosedBy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ESignatureToken).NotEmpty();
        RuleFor(x => x.ClosureNotes).NotEmpty().MaximumLength(1000);
    }
}
