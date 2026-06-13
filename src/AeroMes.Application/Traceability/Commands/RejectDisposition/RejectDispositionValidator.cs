using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.RejectDisposition;

public sealed class RejectDispositionValidator : AbstractValidator<RejectDispositionCommand>
{
    public RejectDispositionValidator()
    {
        RuleFor(x => x.HoldID).NotEmpty();
        RuleFor(x => x.DispositionNotes).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ReleasedBy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ESignatureToken).NotEmpty();
    }
}
