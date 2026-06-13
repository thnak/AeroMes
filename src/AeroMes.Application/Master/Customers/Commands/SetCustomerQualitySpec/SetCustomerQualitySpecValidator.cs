using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Customers.Commands.SetCustomerQualitySpec;

public class SetCustomerQualitySpecValidator : AbstractValidator<SetCustomerQualitySpecCommand>
{
    public SetCustomerQualitySpecValidator(IProductRepository productRepo)
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AqlLevel).MaximumLength(10).When(x => x.AqlLevel != null);
        RuleFor(x => x.AcceptanceCriteria).MaximumLength(500).When(x => x.AcceptanceCriteria != null);
        RuleFor(x => x.SpecialRequirements).MaximumLength(500).When(x => x.SpecialRequirements != null);
        RuleFor(x => x.MaxDefectsPpm).GreaterThanOrEqualTo(0).When(x => x.MaxDefectsPpm.HasValue);
        RuleFor(x => x)
            .Must(x => x.EffectiveTo is null || x.EffectiveFrom is null || x.EffectiveTo >= x.EffectiveFrom)
            .WithMessage("EffectiveTo must be on or after EffectiveFrom.");
        RuleFor(x => x.ProductCode)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (code, ct) => await productRepo.ExistsAsync(code, ct))
            .WithMessage("Product does not exist.");
    }
}
