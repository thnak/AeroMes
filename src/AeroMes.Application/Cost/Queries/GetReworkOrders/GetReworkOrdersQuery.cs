using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetReworkOrders;

public record GetReworkOrdersQuery(ReworkStatus? Status, string? ProductCode, int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<ReworkOrderDto>>;
