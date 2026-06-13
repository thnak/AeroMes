using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetProductionPlans;

public record GetProductionPlansQuery(
    int? PoId,
    ProductionPlanStatus? Status,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<ProductionPlanDto>>;
