using FluentValidation;

namespace AeroMes.Application.Cost.Commands.PostScrap;

public class PostScrapValidator : AbstractValidator<PostScrapCommand>
{
    public PostScrapValidator()
    {
        RuleFor(x => x.WOID).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ScrapQty).GreaterThan(0);
        RuleFor(x => x.MaterialCostPerUnit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LaborCostSunk).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MachineCostSunk).GreaterThanOrEqualTo(0);
    }
}
