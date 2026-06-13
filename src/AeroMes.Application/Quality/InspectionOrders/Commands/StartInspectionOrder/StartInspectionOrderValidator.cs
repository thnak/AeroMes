using FluentValidation;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.StartInspectionOrder;

public class StartInspectionOrderValidator : AbstractValidator<StartInspectionOrderCommand>
{
    public StartInspectionOrderValidator()
    {
        RuleFor(x => x.InspectionOrderId).GreaterThan(0);
    }
}
