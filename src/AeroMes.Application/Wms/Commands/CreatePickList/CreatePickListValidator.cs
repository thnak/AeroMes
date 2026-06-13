using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreatePickList;

public class CreatePickListValidator : AbstractValidator<CreatePickListCommand>
{
    public CreatePickListValidator()
    {
        RuleFor(x => x.ShipmentId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan(0);
    }
}
