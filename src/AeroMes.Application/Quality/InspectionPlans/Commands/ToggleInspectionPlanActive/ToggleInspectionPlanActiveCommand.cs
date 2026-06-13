using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.ToggleInspectionPlanActive;

public record ToggleInspectionPlanActiveCommand(int PlanId, bool Activate) : ICommand<ValidationResult<Unit>>;
