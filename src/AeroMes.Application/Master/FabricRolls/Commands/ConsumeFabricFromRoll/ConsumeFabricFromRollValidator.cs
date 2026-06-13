using FluentValidation;

namespace AeroMes.Application.Master.FabricRolls.Commands.ConsumeFabricFromRoll;

public class ConsumeFabricFromRollValidator : AbstractValidator<ConsumeFabricFromRollCommand>
{
    public ConsumeFabricFromRollValidator()
    {
        RuleFor(x => x.RollID).GreaterThan(0);
        RuleFor(x => x.CutOrderID).GreaterThan(0);
        RuleFor(x => x.MetersConsumed).GreaterThan(0);
        RuleFor(x => x.RecordedBy).NotEmpty().MaximumLength(50);
    }
}
