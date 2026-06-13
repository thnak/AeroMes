using FluentValidation;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.WaiveInspectionOrder;

public class WaiveInspectionOrderValidator : AbstractValidator<WaiveInspectionOrderCommand>
{
    public WaiveInspectionOrderValidator()
    {
        RuleFor(x => x.InspectionOrderId).GreaterThan(0);
        RuleFor(x => x.WaivedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
