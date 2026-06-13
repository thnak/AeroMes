using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Queries.GetDetailedPlans;

public record GetDetailedPlansQuery(
    int? MasterPlanId,
    string? OrgUnit,
    string? Status) : IQuery<IReadOnlyList<DetailedPlanSummaryDto>>;
