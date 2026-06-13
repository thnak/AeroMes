using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.CommissionSerialUnits;

public class CommissionSerialUnitsValidator : AbstractValidator<CommissionSerialUnitsCommand>
{
    public CommissionSerialUnitsValidator()
    {
        RuleFor(x => x.WorkOrderID).GreaterThan(0).WithMessage("Work order ID is required.");
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 10000).WithMessage("Quantity must be between 1 and 10,000.");
        RuleFor(x => x.GTIN).MaximumLength(14).When(x => x.GTIN != null);
        RuleForEach(x => x.ComponentLots).ChildRules(cl =>
        {
            cl.RuleFor(c => c.ComponentLotNumber).NotEmpty().MaximumLength(50);
            cl.RuleFor(c => c.ComponentProductCode).NotEmpty().MaximumLength(50);
        });
    }
}
