using FluentValidation;

namespace AeroMes.Application.Quality.Ncr.Commands.CreateManualNcr;

public class CreateManualNcrValidator : AbstractValidator<CreateManualNcrCommand>
{
    public CreateManualNcrValidator()
    {
        RuleFor(x => x.WorkOrderId).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LotNumber).MaximumLength(100).When(x => x.LotNumber is not null);
        RuleFor(x => x.QtyAffected).GreaterThan(0);
        RuleFor(x => x.Severity).NotEmpty().Must(s => s is "CRITICAL" or "MAJOR" or "MINOR")
            .WithMessage("Severity must be CRITICAL, MAJOR, or MINOR.");
        RuleFor(x => x.CreatedBy).NotEmpty().MaximumLength(100);
    }
}
