using FluentValidation;

namespace AeroMes.Application.Production.Commands.CloseBundleOperation;

public class CloseBundleOperationValidator : AbstractValidator<CloseBundleOperationCommand>
{
    public CloseBundleOperationValidator()
    {
        RuleFor(x => x.BundleBarcode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.QtyOK).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QtyNG).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QtyOK + x.QtyNG).GreaterThan(0)
            .WithName("QtyOK+QtyNG")
            .WithMessage("Total of QtyOK and QtyNG must be greater than zero.");
    }
}
