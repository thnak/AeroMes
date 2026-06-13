using AeroMes.Application.Common;
using AeroMes.Application.Production.MasterPlan.Commands.CreateMasterPlan;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.UpdateMasterPlan;

public record UpdateMasterPlanCommand(
    int MasterPlanId,
    string PlanName,
    string? OrganizationalUnit,
    decimal WorkingHoursPerDay,
    int WorkingDaysPerWeek,
    IReadOnlyList<MasterPlanLineInput> Lines,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
