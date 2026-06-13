using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.CalculateDetailedPlan;

public record CalculateDetailedPlanCommand(
    int DetailPlanId,
    DppDistributionStrategy Strategy,
    int WorkingDaysPerWeek,
    IReadOnlyList<string>? ShiftLabels,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
