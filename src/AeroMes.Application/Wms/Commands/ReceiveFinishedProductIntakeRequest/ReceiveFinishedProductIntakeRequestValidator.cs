using FluentValidation;

namespace AeroMes.Application.Wms.Commands.ReceiveFinishedProductIntakeRequest;

public class ReceiveFinishedProductIntakeRequestValidator
    : AbstractValidator<ReceiveFinishedProductIntakeRequestCommand>
{
    public ReceiveFinishedProductIntakeRequestValidator()
    {
        RuleFor(x => x.IntakeRequestId).GreaterThan(0);
        RuleFor(x => x.ReceiptLines).NotEmpty().WithMessage("Phải cung cấp ít nhất một dòng thực nhập.");

        RuleForEach(x => x.ReceiptLines).ChildRules(line =>
        {
            line.RuleFor(l => l.LineId).GreaterThan(0);
            line.RuleFor(l => l.ActualReceivedQuantity).GreaterThanOrEqualTo(0);
        });
    }
}
