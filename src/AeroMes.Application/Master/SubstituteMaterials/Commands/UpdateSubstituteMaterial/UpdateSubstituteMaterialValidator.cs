using FluentValidation;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.UpdateSubstituteMaterial;

public class UpdateSubstituteMaterialValidator : AbstractValidator<UpdateSubstituteMaterialCommand>
{
    public UpdateSubstituteMaterialValidator()
    {
        RuleFor(x => x.SubstituteId).GreaterThan(0);
        RuleFor(x => x.ConversionRatio).GreaterThan(0);
    }
}
