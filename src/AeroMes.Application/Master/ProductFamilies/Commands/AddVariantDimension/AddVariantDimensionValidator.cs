using FluentValidation;

namespace AeroMes.Application.Master.ProductFamilies.Commands.AddVariantDimension;

public sealed class AddVariantDimensionValidator : AbstractValidator<AddVariantDimensionCommand>
{
    public AddVariantDimensionValidator()
    {
        RuleFor(x => x.FamilyCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DimensionName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(1);
    }
}
