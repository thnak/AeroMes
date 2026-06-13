using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.DistributeMasterPlan;

public record DistributeMasterPlanCommand(
    int MasterPlanId,
    MpsDistributionStrategy Strategy,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
