using FluentValidation;

namespace AeroMes.Application.Production.Commands.CreatePackagingBom;

public class CreatePackagingBomValidator : AbstractValidator<CreatePackagingBomCommand>
{
    public CreatePackagingBomValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.MaterialCode).NotEmpty().MaximumLength(50);
            l.RuleFor(x => x.Quantity).GreaterThan(0);
            l.RuleFor(x => x.UnitCode).NotEmpty().MaximumLength(20);
        });
    }
}
