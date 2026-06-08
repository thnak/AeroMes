using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Machines.Commands.CreateMachine;

public class CreateMachineValidator : AbstractValidator<CreateMachineCommand>
{
    public CreateMachineValidator(IMachineRepository machineRepo, IWorkCenterRepository wcRepo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(30).WithMessage("Code must be at most 30 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await machineRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Machine code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.WorkCenterId)
            .GreaterThan(0).WithMessage("WorkCenter is required.")
            .MustAsync(async (id, ct) => await wcRepo.ExistsAsync(id, ct))
            .WithMessage(x => $"WorkCenter with id {x.WorkCenterId} does not exist.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand must be at most 100 characters.")
            .When(x => x.Brand is not null);

        RuleFor(x => x.Model)
            .MaximumLength(100).WithMessage("Model must be at most 100 characters.")
            .When(x => x.Model is not null);
    }
}
