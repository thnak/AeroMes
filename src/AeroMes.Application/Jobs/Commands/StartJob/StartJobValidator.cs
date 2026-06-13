using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;

namespace AeroMes.Application.Jobs.Commands.StartJob;

public class StartJobValidator : AbstractValidator<StartJobCommand>
{
    public StartJobValidator(IWorkOrderRepository woRepo, IMachineRepository machineRepo, IEmployeeRepository employeeRepo)
    {
        RuleFor(x => x.WorkOrderId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("WorkOrder id is required.")
            .MustAsync(async (id, ct) => await woRepo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"WorkOrder {x.WorkOrderId} does not exist.");

        RuleFor(x => x.MachineCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Machine code is required.")
            .MustAsync(async (code, ct) => await machineRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Machine '{x.MachineCode}' does not exist.");

        RuleFor(x => x.ShiftCode)
            .NotEmpty().WithMessage("Shift code is required.")
            .MaximumLength(20).WithMessage("Shift code must be at most 20 characters.");

        RuleFor(x => x.OperatorId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("OperatorId is required.")
            .MustAsync(async (code, ct) => await employeeRepo.IsActiveAsync(code, ct))
            .WithMessage(x => $"Operator '{x.OperatorId}' is not a registered active employee.");
    }
}
