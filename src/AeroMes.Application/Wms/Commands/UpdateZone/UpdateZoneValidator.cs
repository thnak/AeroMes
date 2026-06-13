using FluentValidation;

namespace AeroMes.Application.Wms.Commands.UpdateZone;

public class UpdateZoneValidator : AbstractValidator<UpdateZoneCommand>
{
    public UpdateZoneValidator()
    {
        RuleFor(x => x.ZoneName).NotEmpty().MaximumLength(100);
    }
}
