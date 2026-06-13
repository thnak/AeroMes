using FluentValidation;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.CreateSubstituteMaterial;

public class CreateSubstituteMaterialValidator : AbstractValidator<CreateSubstituteMaterialCommand>
{
    public CreateSubstituteMaterialValidator()
    {
        RuleFor(x => x.PrimaryMaterialCode).NotEmpty();
        RuleFor(x => x.SubstituteMaterialCode).NotEmpty();
        RuleFor(x => x.ConversionRatio).GreaterThan(0);
    }
}
