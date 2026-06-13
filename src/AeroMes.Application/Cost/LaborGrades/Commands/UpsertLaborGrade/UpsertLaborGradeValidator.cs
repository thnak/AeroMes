using FluentValidation;

namespace AeroMes.Application.Cost.LaborGrades.Commands.UpsertLaborGrade;

public class UpsertLaborGradeValidator : AbstractValidator<UpsertLaborGradeCommand>
{
    public UpsertLaborGradeValidator()
    {
        RuleFor(x => x.GradeCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.GradeName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HourlyRate).GreaterThan(0);
        RuleFor(x => x.OvertimeMultiplier).GreaterThanOrEqualTo(1.0m);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
    }
}
