using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.DeleteInspectionPlan;

public record DeleteInspectionPlanCommand(int PlanId) : ICommand<ValidationResult<Unit>>;
