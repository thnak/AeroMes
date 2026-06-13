using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MRP.Queries.GetMrpList;

public record GetMrpListQuery(
    string? Keyword, int? MasterPlanId, string? Status,
    int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<MrpListDto>>;
