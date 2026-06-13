using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Suppliers.Commands.AddAvlItem;

public class AddAvlItemValidator : AbstractValidator<AddAvlItemCommand>
{
    public AddAvlItemValidator(IProductRepository productRepo)
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CurrencyCode).MaximumLength(10).When(x => x.CurrencyCode != null);
        RuleFor(x => x.AqlLevel).MaximumLength(20).When(x => x.AqlLevel != null);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
        RuleFor(x => x.UnitPrice).GreaterThan(0).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x.LeadTimeDays).GreaterThan(0).When(x => x.LeadTimeDays.HasValue);
        RuleFor(x => x.MinOrderQty).GreaterThan(0).When(x => x.MinOrderQty.HasValue);
        RuleFor(x => x.ProductCode)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (code, ct) => await productRepo.ExistsAsync(code, ct))
            .WithMessage("Product does not exist.");
    }
}
