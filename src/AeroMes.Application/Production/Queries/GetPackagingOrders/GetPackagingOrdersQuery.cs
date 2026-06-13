using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetPackagingOrders;

public record GetPackagingOrdersQuery(
    int? WOID = null,
    PackagingOrderStatus? Status = null) : IQuery<IReadOnlyList<PackagingOrderDto>>;
