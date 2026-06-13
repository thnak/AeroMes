using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Queries.GetProcessSteps;

public record GetProcessStepsQuery(
    string? Keyword, string? Scope, bool? IsActive, int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<ProductionProcessStepDto>>;
