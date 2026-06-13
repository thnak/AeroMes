using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Employees.Commands.SetEmployeeSkill;

public class SetEmployeeSkillValidator : AbstractValidator<SetEmployeeSkillCommand>
{
    public SetEmployeeSkillValidator(IOperationRepository operationRepo)
    {
        RuleFor(x => x.OperationCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.CertificationLevel).InclusiveBetween(1, 5);
        RuleFor(x => x)
            .Must(x => x.ExpiresAt is null || x.ExpiresAt > x.CertifiedAt)
            .WithMessage("ExpiresAt must be after CertifiedAt.");
        RuleFor(x => x.OperationCode)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (code, ct) => await operationRepo.ExistsAsync(code, ct))
            .WithMessage("Operation does not exist.");
    }
}
