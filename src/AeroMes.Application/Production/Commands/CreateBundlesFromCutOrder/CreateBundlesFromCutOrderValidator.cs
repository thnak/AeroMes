using FluentValidation;

namespace AeroMes.Application.Production.Commands.CreateBundlesFromCutOrder;

public class CreateBundlesFromCutOrderValidator : AbstractValidator<CreateBundlesFromCutOrderCommand>
{
    public CreateBundlesFromCutOrderValidator()
    {
        RuleFor(x => x.CutOrderID).GreaterThan(0);
        RuleFor(x => x.BundleSizePerBundle).GreaterThan(0);
    }
}
