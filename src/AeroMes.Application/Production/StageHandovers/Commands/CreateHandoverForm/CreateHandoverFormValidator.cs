using FluentValidation;

namespace AeroMes.Application.Production.StageHandovers.Commands.CreateHandoverForm;

public sealed class CreateHandoverFormValidator : AbstractValidator<CreateHandoverFormCommand>
{
    public CreateHandoverFormValidator()
    {
        RuleFor(x => x.FromWorkOrderId).GreaterThan(0);
        RuleFor(x => x.FromRoutingStepId).GreaterThan(0);
        RuleFor(x => x.ToWorkOrderId).GreaterThan(0);
        RuleFor(x => x.ToRoutingStepId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required.");
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
            l.RuleFor(x => x.Qty).GreaterThan(0);
            l.RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
        });
    }
}
