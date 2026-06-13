using FluentValidation;

namespace AeroMes.Application.Master.ProductFamilies.Commands.AddDimensionValue;

public sealed class AddDimensionValueValidator : AbstractValidator<AddDimensionValueCommand>
{
    public AddDimensionValueValidator()
    {
        RuleFor(x => x.FamilyCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DimensionId).GreaterThan(0);
        RuleFor(x => x.ValueCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ValueLabel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(1);
    }
}
