using FluentValidation;

namespace AeroMes.Application.Production.Commands.CreateCutOrder;

public class CreateCutOrderValidator : AbstractValidator<CreateCutOrderCommand>
{
    public CreateCutOrderValidator()
    {
        RuleFor(x => x.CutOrderCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WOID).GreaterThan(0);
        RuleFor(x => x.StyleCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ColorCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FabricProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ShadeCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.PlyCount).GreaterThan(0);
        RuleFor(x => x.SpreadLengthMeters).GreaterThan(0);
        RuleFor(x => x.FabricWidthCm).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty()
            .Must(lines => lines.Any(l => l.QuantityToCut > 0))
            .WithMessage("At least one size line must have QuantityToCut > 0.");
    }
}
