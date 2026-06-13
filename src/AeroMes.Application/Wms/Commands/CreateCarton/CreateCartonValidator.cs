using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateCarton;

public class CreateCartonValidator : AbstractValidator<CreateCartonCommand>
{
    public CreateCartonValidator()
    {
        RuleFor(x => x.ShipmentId).GreaterThan(0);
    }
}
