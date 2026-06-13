using FluentValidation;

namespace AeroMes.Application.Production.Commands.CreatePackagingOrder;

public class CreatePackagingOrderValidator : AbstractValidator<CreatePackagingOrderCommand>
{
    public CreatePackagingOrderValidator()
    {
        RuleFor(x => x.WOID).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PlannedQty).GreaterThan(0);
    }
}
