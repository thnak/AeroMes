using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public class StartDowntimeValidator : AbstractValidator<StartDowntimeCommand>
{
    public StartDowntimeValidator(IMachineRepository machineRepo)
    {
        RuleFor(x => x.MachineCode)
            .NotEmpty().WithMessage("Machine code is required.")
            .MustAsync(async (code, ct) => await machineRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Machine '{x.MachineCode}' does not exist.");

        RuleFor(x => x.ReasonCode)
            .NotEmpty().WithMessage("Reason code is required.")
            .MaximumLength(30).WithMessage("Reason code must be at most 30 characters.");

        RuleFor(x => x.ReasonName)
            .MaximumLength(100).WithMessage("Reason name must be at most 100 characters.")
            .When(x => x.ReasonName is not null);

        RuleFor(x => x.OperatorId)
            .NotEmpty().WithMessage("OperatorId is required.");

        RuleFor(x => x.StartTime)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Start time cannot be in the future.");
    }
}
