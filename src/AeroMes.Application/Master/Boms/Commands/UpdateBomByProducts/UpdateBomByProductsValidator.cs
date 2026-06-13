using FluentValidation;

namespace AeroMes.Application.Master.Boms.Commands.UpdateBomByProducts;

public class UpdateBomByProductsValidator : AbstractValidator<UpdateBomByProductsCommand>
{
    public UpdateBomByProductsValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty();
        RuleFor(x => x.Version).NotEmpty();
        RuleForEach(x => x.ByProducts).ChildRules(r =>
        {
            r.RuleFor(b => b.ByProductCode).NotEmpty();
            r.RuleFor(b => b.Quantity).GreaterThan(0);
            r.RuleFor(b => b.UoMCode).NotEmpty();
        });
    }
}
