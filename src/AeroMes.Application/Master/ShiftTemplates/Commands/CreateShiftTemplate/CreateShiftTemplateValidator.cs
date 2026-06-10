using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.CreateShiftTemplate;

public class CreateShiftTemplateValidator : AbstractValidator<CreateShiftTemplateCommand>
{
    public CreateShiftTemplateValidator(IShiftTemplateRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"ShiftTemplate code '{x.Code}' already exists.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.ValidDays)
            .Must(d => d != 0).WithMessage("At least one valid day must be selected.");
    }
}
