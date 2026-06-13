using AeroMes.Application.Quality.InspectionPlans;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Queries.GetInspectionPlans;

public record GetInspectionPlansQuery(
    int? RoutingStepId,
    string? ProductCode,
    bool? IsActive) : IQuery<IReadOnlyList<InspectionPlanListDto>>;
