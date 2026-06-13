using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Queries.GetMasterPlans;

public record GetMasterPlansQuery(
    string? OrgUnit,
    string? Status,
    DateOnly? From,
    DateOnly? To) : IQuery<IReadOnlyList<MasterPlanSummaryDto>>;
