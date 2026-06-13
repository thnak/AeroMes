using AeroMes.Domain.Master;
using FluentValidation;

namespace AeroMes.Application.Master.ProductFamilies.Commands.CreateProductFamily;

public sealed class CreateProductFamilyValidator : AbstractValidator<CreateProductFamilyCommand>
{
    public CreateProductFamilyValidator()
    {
        RuleFor(x => x.FamilyCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FamilyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BaseProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Industry).NotEmpty()
            .Must(i => FamilyIndustries.All.Contains(i))
            .WithMessage(x => $"Industry must be one of: {string.Join(", ", FamilyIndustries.All)}.");
    }
}
