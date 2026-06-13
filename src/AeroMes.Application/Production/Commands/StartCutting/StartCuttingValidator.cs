using FluentValidation;

namespace AeroMes.Application.Production.Commands.StartCutting;

public class StartCuttingValidator : AbstractValidator<StartCuttingCommand>
{
    public StartCuttingValidator()
    {
        RuleFor(x => x.CutOrderID).GreaterThan(0);
        RuleFor(x => x.OperatorID).NotEmpty().MaximumLength(50);
    }
}
