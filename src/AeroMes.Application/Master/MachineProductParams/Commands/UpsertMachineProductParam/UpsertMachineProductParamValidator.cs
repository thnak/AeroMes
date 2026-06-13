using FluentValidation;

namespace AeroMes.Application.Master.MachineProductParams.Commands.UpsertMachineProductParam;

public class UpsertMachineProductParamValidator : AbstractValidator<UpsertMachineProductParamCommand>
{
    public UpsertMachineProductParamValidator()
    {
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ParamName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).MaximumLength(20);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
