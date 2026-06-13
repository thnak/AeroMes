using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.UpdateInspectionPlan;

public record UpdateInspectionPlanCommand(
    int PlanId,
    string Name,
    int RoutingStepId,
    string? ProductCode,
    string SamplingMethod,
    int? SampleSize,
    int AcceptNumber,
    int RejectNumber,
    string InspectionType,
    string? Notes) : ICommand<ValidationResult<Unit>>;
