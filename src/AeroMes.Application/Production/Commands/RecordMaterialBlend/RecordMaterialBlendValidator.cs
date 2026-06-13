using FluentValidation;

namespace AeroMes.Application.Production.Commands.RecordMaterialBlend;

public class RecordMaterialBlendValidator : AbstractValidator<RecordMaterialBlendCommand>
{
    public RecordMaterialBlendValidator()
    {
        RuleFor(x => x.JobID).GreaterThan(0);
        RuleFor(x => x.ResinProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VirginLotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VirginQtyKg).GreaterThan(0).WithMessage("Virgin quantity must be positive.");
        RuleFor(x => x.RegrindQtyKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RegrindLotNumber)
            .NotEmpty().MaximumLength(50)
            .When(x => x.RegrindQtyKg > 0)
            .WithMessage("Regrind lot number is required when regrind quantity is greater than zero.");
    }
}
