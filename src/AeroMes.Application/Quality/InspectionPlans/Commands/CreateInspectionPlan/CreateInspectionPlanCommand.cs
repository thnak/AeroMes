using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.CreateInspectionPlan;

public record CreateInspectionPlanCommand(
    string Code,
    string Name,
    int RoutingStepId,
    string? ProductCode,
    string SamplingMethod,
    int? SampleSize,
    int AcceptNumber,
    int RejectNumber,
    string InspectionType,
    string? Notes,
    string CreatedBy) : ICommand<ValidationResult<int>>;
