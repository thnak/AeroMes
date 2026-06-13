using FluentValidation;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.FailInspectionOrder;

public class FailInspectionOrderValidator : AbstractValidator<FailInspectionOrderCommand>
{
    public FailInspectionOrderValidator()
    {
        RuleFor(x => x.InspectionOrderId).GreaterThan(0);
    }
}
