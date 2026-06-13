using FluentValidation;

namespace AeroMes.Application.Wms.Commands.AddGrnLine;

public class AddGrnLineValidator : AbstractValidator<AddGrnLineCommand>
{
    public AddGrnLineValidator()
    {
        RuleFor(x => x.GrnId).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReceivedQty).GreaterThan(0);
        RuleFor(x => x.GrossWeightKg).GreaterThan(0).When(x => x.GrossWeightKg.HasValue);
    }
}
