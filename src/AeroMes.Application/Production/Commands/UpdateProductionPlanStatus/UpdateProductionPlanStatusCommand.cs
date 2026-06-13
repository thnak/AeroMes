using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.UpdateProductionPlanStatus;

public record UpdateProductionPlanStatusCommand(
    int PlanId,
    ProductionPlanStatus NewStatus) : ICommand<ValidationResult<Unit>>;
