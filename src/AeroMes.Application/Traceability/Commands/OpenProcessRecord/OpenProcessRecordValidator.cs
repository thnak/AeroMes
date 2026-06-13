using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.OpenProcessRecord;

public sealed class OpenProcessRecordValidator : AbstractValidator<OpenProcessRecordCommand>
{
    public OpenProcessRecordValidator()
    {
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WorkOrderID).GreaterThan(0);
        RuleFor(x => x.RoutingStepID).GreaterThan(0);
        RuleFor(x => x.StepName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OperatorCode).NotEmpty().MaximumLength(50);
    }
}
