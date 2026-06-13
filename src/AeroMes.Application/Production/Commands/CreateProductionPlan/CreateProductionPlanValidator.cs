using FluentValidation;

namespace AeroMes.Application.Production.Commands.CreateProductionPlan;

public class CreateProductionPlanValidator : AbstractValidator<CreateProductionPlanCommand>
{
    public CreateProductionPlanValidator()
    {
        RuleFor(x => x.PoId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Kế hoạch phải có ít nhất một dòng sản phẩm.");
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.ProductCode).NotEmpty();
            l.RuleFor(x => x.PlannedQty).GreaterThan(0);
        });
    }
}
