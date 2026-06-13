using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CompletePickList;

public class CompletePickListValidator : AbstractValidator<CompletePickListCommand>
{
    public CompletePickListValidator()
    {
        RuleFor(x => x.PickListId).GreaterThan(0);
    }
}
