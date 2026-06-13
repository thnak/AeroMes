using FluentValidation;

namespace AeroMes.Application.Cost.ItemCosts.Commands.SetItemStandardCost;

public class SetItemStandardCostValidator : AbstractValidator<SetItemStandardCostCommand>
{
    public SetItemStandardCostValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitCost).GreaterThan(0);
        RuleFor(x => x.CostUoM).NotEmpty().MaximumLength(20);
    }
}
