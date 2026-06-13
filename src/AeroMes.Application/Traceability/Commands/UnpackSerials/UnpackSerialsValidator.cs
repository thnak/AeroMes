using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.UnpackSerials;

public class UnpackSerialsValidator : AbstractValidator<UnpackSerialsCommand>
{
    public UnpackSerialsValidator()
    {
        RuleFor(x => x.SSCC).NotEmpty().MaximumLength(20);
        RuleFor(x => x.OperatorCode).NotEmpty().MaximumLength(50);
    }
}
