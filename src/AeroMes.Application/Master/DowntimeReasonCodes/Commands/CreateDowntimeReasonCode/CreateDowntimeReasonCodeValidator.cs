using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.CreateDowntimeReasonCode;

public class CreateDowntimeReasonCodeValidator : AbstractValidator<CreateDowntimeReasonCodeCommand>
{
    public CreateDowntimeReasonCodeValidator(IDowntimeReasonCodeRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(30)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"DowntimeReasonCode '{x.Code}' already exists.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);

        RuleFor(x => x.SlaMinutes)
            .GreaterThan(0).WithMessage("SLA minutes must be greater than zero.")
            .When(x => x.SlaMinutes.HasValue);
    }
}
