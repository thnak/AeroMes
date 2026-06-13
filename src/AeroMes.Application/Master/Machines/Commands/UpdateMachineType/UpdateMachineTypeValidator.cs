using AeroMes.Application.Localization;
using AeroMes.Domain.Master;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineType;

public sealed class UpdateMachineTypeValidator : AbstractValidator<UpdateMachineTypeCommand>
{
    public UpdateMachineTypeValidator(IStringLocalizer<SharedResources> L)
    {
        RuleFor(x => x.MachineCode).NotEmpty();
        RuleFor(x => x.MachineType)
            .NotEmpty()
            .Must(t => MachineTypes.All.Contains(t))
            .WithMessage(_ => L["InvalidValue"].Value);
    }
}
