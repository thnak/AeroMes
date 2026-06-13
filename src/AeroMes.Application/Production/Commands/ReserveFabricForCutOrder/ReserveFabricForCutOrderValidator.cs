using FluentValidation;

namespace AeroMes.Application.Production.Commands.ReserveFabricForCutOrder;

public class ReserveFabricForCutOrderValidator : AbstractValidator<ReserveFabricForCutOrderCommand>
{
    public ReserveFabricForCutOrderValidator()
    {
        RuleFor(x => x.CutOrderID).GreaterThan(0);
        RuleFor(x => x.RollIDs).NotEmpty();
    }
}
