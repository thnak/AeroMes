using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.BulkHoldFromForwardTrace;

public sealed class BulkHoldFromForwardTraceValidator : AbstractValidator<BulkHoldFromForwardTraceCommand>
{
    public BulkHoldFromForwardTraceValidator()
    {
        RuleFor(x => x.SuspectLotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.HoldReference).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InitiatedBy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MaxDepth).InclusiveBetween(1, 50);
    }
}
