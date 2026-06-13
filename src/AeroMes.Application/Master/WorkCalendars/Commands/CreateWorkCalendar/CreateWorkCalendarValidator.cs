using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.WorkCalendars.Commands.CreateWorkCalendar;

public class CreateWorkCalendarValidator : AbstractValidator<CreateWorkCalendarCommand>
{
    public CreateWorkCalendarValidator(IWorkCalendarRepository repo, IWorkShiftRepository shiftRepo)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        RuleFor(x => x).Cascade(CascadeMode.Stop).MustAsync(async (cmd, ct) =>
                !await repo.CodeExistsAsync(cmd.Code, ct))
            .WithName("Code")
            .WithMessage("Calendar code already exists.")
            .OverridePropertyName("Code");
        RuleFor(x => x.Days).NotEmpty()
            .WithMessage("At least one working day with a shift is required.");
        RuleForEach(x => x.Days).ChildRules(d =>
        {
            d.RuleFor(x => x.DayOfWeek).IsInEnum();
            d.RuleForEach(x => x.Shifts).ChildRules(s =>
            {
                s.RuleFor(x => x.WorkShiftId).GreaterThan(0);
                s.RuleFor(x => x.Sequence).GreaterThanOrEqualTo(1);
            });
        });
    }
}
