using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Queries.GetProductionProcesses;

public record GetProductionProcessesQuery(
    string? Keyword, string? ProcessType, bool? IsActive,
    int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<ProductionProcessListDto>>;
