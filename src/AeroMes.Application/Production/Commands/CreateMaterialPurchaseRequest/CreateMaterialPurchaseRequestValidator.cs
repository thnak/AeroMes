using FluentValidation;

namespace AeroMes.Application.Production.Commands.CreateMaterialPurchaseRequest;

public class CreateMaterialPurchaseRequestValidator : AbstractValidator<CreateMaterialPurchaseRequestCommand>
{
    public CreateMaterialPurchaseRequestValidator()
    {
        RuleFor(x => x.Requestor).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Yêu cầu phải có ít nhất một dòng vật tư.");
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.MaterialCode).NotEmpty();
            l.RuleFor(x => x.MaterialName).NotEmpty();
            l.RuleFor(x => x.UnitOfMeasure).NotEmpty();
            l.RuleFor(x => x.RequiredQty).GreaterThan(0);
        });
    }
}
