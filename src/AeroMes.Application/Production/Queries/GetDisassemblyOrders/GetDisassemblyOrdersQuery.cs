using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetDisassemblyOrders;

public record GetDisassemblyOrdersQuery(
    string? SourceProductCode = null,
    DisassemblyOrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<DisassemblyOrderDto>>;
