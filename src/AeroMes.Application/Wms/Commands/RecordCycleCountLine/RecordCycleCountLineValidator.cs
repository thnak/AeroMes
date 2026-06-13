using FluentValidation;

namespace AeroMes.Application.Wms.Commands.RecordCycleCountLine;

public class RecordCycleCountLineValidator : AbstractValidator<RecordCycleCountLineCommand>
{
    public RecordCycleCountLineValidator()
    {
        RuleFor(x => x.CountedQty).GreaterThanOrEqualTo(0).WithMessage("Số lượng kiểm kê không được âm.");
    }
}
