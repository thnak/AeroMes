using AeroMes.Application.Quality.InspectionPlans;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Queries.GetInspectionPlanDetail;

public record GetInspectionPlanDetailQuery(int PlanId) : IQuery<InspectionPlanDetailDto?>;
