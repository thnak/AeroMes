using AeroMes.Application.Common;
using AeroMes.Application.Production.DetailedPlan.Commands.CreateDetailedPlan;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.UpdateDetailedPlan;

public record UpdateDetailedPlanCommand(
    int DetailPlanId,
    string PlanName,
    DppGranularity Granularity,
    IReadOnlyList<DppProductLineInput> ProductLines,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
