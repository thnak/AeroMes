using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateRack;

public class CreateRackValidator : AbstractValidator<CreateRackCommand>
{
    public CreateRackValidator()
    {
        RuleFor(x => x.AisleId).GreaterThan(0);
        RuleFor(x => x.RackCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.MaxWeightKg).GreaterThan(0).When(x => x.MaxWeightKg.HasValue);
    }
}
