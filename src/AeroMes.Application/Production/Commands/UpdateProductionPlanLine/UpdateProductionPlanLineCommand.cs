using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.UpdateProductionPlanLine;

public record UpdateProductionPlanLineCommand(
    int PlanId,
    int PlanLineId,
    string? TeamCode,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate) : ICommand<ValidationResult<Unit>>;
