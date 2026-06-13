using FluentValidation;

namespace AeroMes.Application.Master.FabricRolls.Commands.QuarantineFabricRoll;

public class QuarantineFabricRollValidator : AbstractValidator<QuarantineFabricRollCommand>
{
    public QuarantineFabricRollValidator()
    {
        RuleFor(x => x.RollID).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
    }
}
