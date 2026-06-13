using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.AssignMoldToJob;

public class AssignMoldToJobValidator : AbstractValidator<AssignMoldToJobCommand>
{
    public AssignMoldToJobValidator()
    {
        RuleFor(x => x.MoldCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WOID).GreaterThan(0);
        RuleFor(x => x.JobID).GreaterThan(0);
        RuleFor(x => x.AssignedBy).NotEmpty().MaximumLength(50);
    }
}
