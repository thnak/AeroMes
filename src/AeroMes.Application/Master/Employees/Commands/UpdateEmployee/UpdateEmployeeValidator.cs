using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator(IWorkCenterRepository workCenterRepo)
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Department).MaximumLength(100).When(x => x.Department != null);
        RuleFor(x => x.DefaultWorkCenterId)
            .MustAsync(async (id, ct) => await workCenterRepo.ExistsAsync(id!.Value, ct))
            .WithMessage("Default work center does not exist.")
            .When(x => x.DefaultWorkCenterId.HasValue);
    }
}
