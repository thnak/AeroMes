using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachine;

public class UpdateMachineValidator : AbstractValidator<UpdateMachineCommand>
{
    public UpdateMachineValidator(IMachineRepository machineRepo, IWorkCenterRepository wcRepo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Machine code is required.")
            .MustAsync(async (code, ct) => await machineRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Machine '{x.Code}' does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.WorkCenterId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("WorkCenter is required.")
            .MustAsync(async (id, ct) => await wcRepo.ExistsAsync(id, ct))
            .WithMessage(x => $"WorkCenter {x.WorkCenterId} does not exist.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand must be at most 100 characters.")
            .When(x => x.Brand is not null);

        RuleFor(x => x.Model)
            .MaximumLength(100).WithMessage("Model must be at most 100 characters.")
            .When(x => x.Model is not null);

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required.");
    }
}
