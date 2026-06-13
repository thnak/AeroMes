using FluentValidation;

namespace AeroMes.Application.Wms.Commands.ReceiveRma;

public class ReceiveRmaValidator : AbstractValidator<ReceiveRmaCommand>
{
    public ReceiveRmaValidator()
    {
        RuleFor(x => x.LineReceipts).NotEmpty();
        RuleForEach(x => x.LineReceipts).ChildRules(l =>
        {
            l.RuleFor(x => x.LineId).GreaterThan(0);
            l.RuleFor(x => x.ReceivedQty).GreaterThanOrEqualTo(0);
        });
    }
}
