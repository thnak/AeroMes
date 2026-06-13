using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateShipmentOrder;

public class CreateShipmentOrderValidator : AbstractValidator<CreateShipmentOrderCommand>
{
    public CreateShipmentOrderValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SoId).GreaterThan(0).When(x => x.SoId.HasValue);
    }
}
