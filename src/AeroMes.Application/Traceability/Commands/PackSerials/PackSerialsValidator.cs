using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.PackSerials;

public class PackSerialsValidator : AbstractValidator<PackSerialsCommand>
{
    public PackSerialsValidator()
    {
        RuleFor(x => x.SerialNumbers).NotEmpty().WithMessage("At least one serial number is required.");
        RuleForEach(x => x.SerialNumbers).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CaseSSCC).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PalletSSCC).MaximumLength(20).When(x => x.PalletSSCC != null);
        RuleFor(x => x.OperatorCode).NotEmpty().MaximumLength(50);
    }
}
