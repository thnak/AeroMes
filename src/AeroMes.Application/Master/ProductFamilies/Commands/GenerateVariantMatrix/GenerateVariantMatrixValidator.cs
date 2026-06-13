using FluentValidation;

namespace AeroMes.Application.Master.ProductFamilies.Commands.GenerateVariantMatrix;

public sealed class GenerateVariantMatrixValidator : AbstractValidator<GenerateVariantMatrixCommand>
{
    public GenerateVariantMatrixValidator()
    {
        RuleFor(x => x.FamilyCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductCodePrefix).NotEmpty().MaximumLength(30);
    }
}
