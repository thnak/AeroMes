using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateAisle;

public class CreateAisleValidator : AbstractValidator<CreateAisleCommand>
{
    public CreateAisleValidator()
    {
        RuleFor(x => x.ZoneId).GreaterThan(0);
        RuleFor(x => x.AisleCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PickSequence).GreaterThanOrEqualTo(0);
    }
}
