using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateCycleCountPlan;

public record CreateCycleCountPlanCommand(
    CycleCountPlanType PlanType,
    DateOnly ScheduledDate,
    string? Notes,
    string? CreatedBy
) : ICommand<ValidationResult<CycleCountPlanCreatedResult>>;

public record CycleCountPlanCreatedResult(int PlanId, string PlanCode);
