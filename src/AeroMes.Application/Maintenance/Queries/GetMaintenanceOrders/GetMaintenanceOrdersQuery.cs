using AeroMes.Application.Common;
using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Maintenance.Queries.GetMaintenanceOrders;

public record GetMaintenanceOrdersQuery(
    string? MachineCode,
    MaintenanceOrderStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<MaintenanceOrderDto>>;
