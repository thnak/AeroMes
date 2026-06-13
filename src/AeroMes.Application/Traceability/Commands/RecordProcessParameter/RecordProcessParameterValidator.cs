using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.RecordProcessParameter;

public sealed class RecordProcessParameterValidator : AbstractValidator<RecordProcessParameterCommand>
{
    public RecordProcessParameterValidator()
    {
        RuleFor(x => x.ProcessRecordID).NotEmpty();
        RuleFor(x => x.ParameterName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ActualValue).NotEmpty().MaximumLength(100);
    }
}
