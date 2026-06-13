using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateProductionPlan;

public record PlanLineInput(
    string ProductCode,
    decimal PlannedQty,
    string? TeamCode = null,
    DateTime? PlannedStartDate = null,
    DateTime? PlannedEndDate = null);

public record CreateProductionPlanCommand(
    int PoId,
    PlanAllocationMethod AllocationMethod,
    IReadOnlyList<PlanLineInput> Lines,
    string? Notes = null) : ICommand<ValidationResult<int>>;
