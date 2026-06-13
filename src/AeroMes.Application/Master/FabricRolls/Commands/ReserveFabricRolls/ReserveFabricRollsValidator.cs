using FluentValidation;

namespace AeroMes.Application.Master.FabricRolls.Commands.ReserveFabricRolls;

public class ReserveFabricRollsValidator : AbstractValidator<ReserveFabricRollsCommand>
{
    public ReserveFabricRollsValidator()
    {
        RuleFor(x => x.CutOrderID).GreaterThan(0);
        RuleFor(x => x.RollIDs).NotEmpty();
    }
}
