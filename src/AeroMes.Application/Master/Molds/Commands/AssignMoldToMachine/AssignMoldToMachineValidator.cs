using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.AssignMoldToMachine;

public class AssignMoldToMachineValidator : AbstractValidator<AssignMoldToMachineCommand>
{
    public AssignMoldToMachineValidator(IMachineRepository machineRepo)
    {
        RuleFor(x => x.MoldCode).NotEmpty();
        RuleFor(x => x.MachineCode)
            .NotEmpty()
            .MustAsync(machineRepo.ExistsAsync)
            .WithMessage(x => $"Máy '{x.MachineCode}' không tồn tại.");
    }
}
