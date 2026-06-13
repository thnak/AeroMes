using FluentValidation;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.AssignInspectionOrder;

public class AssignInspectionOrderValidator : AbstractValidator<AssignInspectionOrderCommand>
{
    public AssignInspectionOrderValidator()
    {
        RuleFor(x => x.InspectionOrderId).GreaterThan(0);
        RuleFor(x => x.InspectorCode).NotEmpty().MaximumLength(100);
    }
}
