using FluentValidation;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineCapacity;

public class UpdateMachineCapacityValidator : AbstractValidator<UpdateMachineCapacityCommand>
{
    public UpdateMachineCapacityValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MachineCategory).MaximumLength(30);
        RuleFor(x => x.TargetOeePct).InclusiveBetween(0, 100).When(x => x.TargetOeePct.HasValue);
        RuleFor(x => x.TheoreticalCapacityPerHour).GreaterThan(0).When(x => x.TheoreticalCapacityPerHour.HasValue);
        RuleFor(x => x.PlannedDowntimeMinPerShift).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HourlyCostRate).GreaterThanOrEqualTo(0).When(x => x.HourlyCostRate.HasValue);
        RuleFor(x => x.OpcUaNodeId).MaximumLength(200);
        RuleFor(x => x.CertificationCode).MaximumLength(30);
        RuleFor(x => x.MaxOperators).GreaterThan((byte)0);
    }
}
