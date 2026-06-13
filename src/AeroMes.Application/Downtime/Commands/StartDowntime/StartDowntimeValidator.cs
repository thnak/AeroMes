using AeroMes.Application.Localization;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public class StartDowntimeValidator : AbstractValidator<StartDowntimeCommand>
{
    public StartDowntimeValidator(IMachineRepository machineRepo, IStringLocalizer<SharedResources> L)
    {
        RuleFor(x => x.MachineCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(_ => L["Required"].Value)
            .MustAsync(async (code, ct) => await machineRepo.ExistsAsync(code, ct))
            .WithMessage(_ => L["MachineNotFound"].Value);

        RuleFor(x => x.ReasonCode)
            .NotEmpty().WithMessage(_ => L["Required"].Value)
            .MaximumLength(30);

        RuleFor(x => x.ReasonName)
            .MaximumLength(100)
            .When(x => x.ReasonName is not null);

        RuleFor(x => x.OperatorId)
            .NotEmpty().WithMessage(_ => L["Required"].Value);

        RuleFor(x => x.StartTime)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Thời gian bắt đầu không được trong tương lai.");
    }
}
