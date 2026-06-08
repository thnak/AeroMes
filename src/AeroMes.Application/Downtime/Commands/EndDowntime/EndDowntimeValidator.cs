using AeroMes.Domain.Production.Repositories;
using FluentValidation;

namespace AeroMes.Application.Downtime.Commands.EndDowntime;

public class EndDowntimeValidator : AbstractValidator<EndDowntimeCommand>
{
    public EndDowntimeValidator(IDowntimeLogRepository repo)
    {
        RuleFor(x => x.DowntimeLogId)
            .GreaterThan(0).WithMessage("DowntimeLog id is required.")
            .MustAsync(async (id, ct) => await repo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"DowntimeLog {x.DowntimeLogId} does not exist.");

        RuleFor(x => x.EndTime)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("End time cannot be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must be at most 500 characters.")
            .When(x => x.Notes is not null);
    }
}
