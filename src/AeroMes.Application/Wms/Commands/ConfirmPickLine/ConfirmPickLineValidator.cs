using FluentValidation;

namespace AeroMes.Application.Wms.Commands.ConfirmPickLine;

public class ConfirmPickLineValidator : AbstractValidator<ConfirmPickLineCommand>
{
    public ConfirmPickLineValidator()
    {
        RuleFor(x => x.PickLineId).GreaterThan(0);
        RuleFor(x => x.PickListId).GreaterThan(0);
        RuleFor(x => x.ActualPickedQty).GreaterThanOrEqualTo(0);
    }
}
