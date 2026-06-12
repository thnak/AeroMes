using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Employees.Commands.AddShiftAssignment;

public class AddShiftAssignmentValidator : AbstractValidator<AddShiftAssignmentCommand>
{
    public AddShiftAssignmentValidator(IWorkCenterRepository workCenterRepo, IShiftTemplateRepository shiftRepo)
    {
        RuleFor(x => x.ShiftCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.WorkCenterId)
            .MustAsync(async (id, ct) => await workCenterRepo.ExistsAsync(id, ct))
            .WithMessage("Work center does not exist.");
        RuleFor(x => x.ShiftCode)
            .MustAsync(async (code, ct) => await shiftRepo.CodeExistsAsync(code, ct))
            .WithMessage("Shift template does not exist.");
    }
}
