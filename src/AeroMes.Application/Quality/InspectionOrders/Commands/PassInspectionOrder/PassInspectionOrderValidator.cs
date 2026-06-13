using FluentValidation;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.PassInspectionOrder;

public class PassInspectionOrderValidator : AbstractValidator<PassInspectionOrderCommand>
{
    public PassInspectionOrderValidator()
    {
        RuleFor(x => x.InspectionOrderId).GreaterThan(0);
    }
}
