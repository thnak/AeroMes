using FluentValidation;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolUsage;

public class RecordToolUsageValidator : AbstractValidator<RecordToolUsageCommand>
{
    public RecordToolUsageValidator()
    {
        RuleFor(x => x.ToolCode).NotEmpty();
        RuleFor(x => x.Count).GreaterThan(0);
    }
}
