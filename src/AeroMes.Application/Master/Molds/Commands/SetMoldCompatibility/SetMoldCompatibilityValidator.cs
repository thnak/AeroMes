using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.SetMoldCompatibility;

public class SetMoldCompatibilityValidator : AbstractValidator<SetMoldCompatibilityCommand>
{
    public SetMoldCompatibilityValidator()
    {
        RuleFor(x => x.MoldCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(255).When(x => x.Notes != null);
    }
}
