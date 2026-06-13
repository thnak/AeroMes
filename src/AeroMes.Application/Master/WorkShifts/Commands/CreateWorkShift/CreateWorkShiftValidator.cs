using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.WorkShifts.Commands.CreateWorkShift;

public class CreateWorkShiftValidator : AbstractValidator<CreateWorkShiftCommand>
{
    public CreateWorkShiftValidator(IWorkShiftRepository repo)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartTime).NotEqual(x => x.EndTime)
            .WithMessage("Start time and end time must differ.");
        RuleFor(x => x).Cascade(CascadeMode.Stop).MustAsync(async (cmd, ct) =>
                !await repo.CodeExistsAsync(cmd.Code, ct))
            .WithName("Code")
            .WithMessage("Shift code '{PropertyValue}' already exists.")
            .OverridePropertyName("Code");
        RuleForEach(x => x.Breaks).ChildRules(b =>
        {
            b.RuleFor(x => x.BreakStart).NotEqual(x => x.BreakEnd)
                .WithMessage("Break start and end times must differ.");
        });
    }
}
