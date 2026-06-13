using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteCycleCountPlan;

public record DeleteCycleCountPlanCommand(
    int PlanId,
    string? DeletedBy
) : ICommand<ValidationResult<Unit>>;
