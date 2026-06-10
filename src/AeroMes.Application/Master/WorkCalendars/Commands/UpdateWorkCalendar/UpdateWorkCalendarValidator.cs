using AeroMes.Application.Master.WorkCalendars.Commands.CreateWorkCalendar;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.WorkCalendars.Commands.UpdateWorkCalendar;

public class UpdateWorkCalendarValidator : AbstractValidator<UpdateWorkCalendarCommand>
{
    public UpdateWorkCalendarValidator(IWorkCalendarRepository repo)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        RuleFor(x => x.WorkCalendarId).MustAsync(async (id, ct) =>
                await repo.GetByIdAsync(id, ct) is not null)
            .WithMessage("Work calendar not found.");
        RuleFor(x => x.Days).NotEmpty()
            .WithMessage("At least one day configuration is required.");
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
