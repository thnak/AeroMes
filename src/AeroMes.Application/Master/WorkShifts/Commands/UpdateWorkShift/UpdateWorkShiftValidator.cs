using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.WorkShifts.Commands.UpdateWorkShift;

public class UpdateWorkShiftValidator : AbstractValidator<UpdateWorkShiftCommand>
{
    public UpdateWorkShiftValidator(IWorkShiftRepository repo)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartTime).NotEqual(x => x.EndTime)
            .WithMessage("Start time and end time must differ.");
        RuleFor(x => x.WorkShiftId).MustAsync(async (id, ct) =>
                await repo.GetByIdAsync(id, ct) is not null)
            .WithMessage("Work shift not found.");
        RuleForEach(x => x.Breaks).ChildRules(b =>
        {
            b.RuleFor(x => x.BreakStart).NotEqual(x => x.BreakEnd)
                .WithMessage("Break start and end times must differ.");
        });
    }
}
