using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateZone;

public class CreateZoneValidator : AbstractValidator<CreateZoneCommand>
{
    public CreateZoneValidator()
    {
        RuleFor(x => x.ZoneCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ZoneName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StorageLocationId).GreaterThan(0);
    }
}
