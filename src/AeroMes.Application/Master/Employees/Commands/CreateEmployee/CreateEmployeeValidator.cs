using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Employees.Commands.CreateEmployee;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator(IEmployeeRepository repo, IWorkCenterRepository workCenterRepo)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Department).MaximumLength(100).When(x => x.Department != null);
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage("Employee code already exists.");
        RuleFor(x => x.DefaultWorkCenterId)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (id, ct) => await workCenterRepo.ExistsAsync(id!.Value, ct))
            .WithMessage("Default work center does not exist.")
            .When(x => x.DefaultWorkCenterId.HasValue);
    }
}
