using AeroMes.Domain.Traceability;
using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.CloseProcessRecord;

public sealed class CloseProcessRecordValidator : AbstractValidator<CloseProcessRecordCommand>
{
    public CloseProcessRecordValidator()
    {
        RuleFor(x => x.ProcessRecordID).NotEmpty();
        RuleFor(x => x.Outcome).Must(o => o != StepOutcome.InProgress)
            .WithMessage("Outcome cannot be 'InProgress' when closing a record.");
        RuleFor(x => x.OutputLotNumber).MaximumLength(50).When(x => x.OutputLotNumber is not null);
    }
}
