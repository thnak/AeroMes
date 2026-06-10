using FluentValidation;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.UpdateShiftTemplate;

public class UpdateShiftTemplateValidator : AbstractValidator<UpdateShiftTemplateCommand>
{
    public UpdateShiftTemplateValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ValidDays)
            .Must(d => d != 0).WithMessage("At least one valid day must be selected.");
    }
}
