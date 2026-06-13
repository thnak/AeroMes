using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Queries.GetWarehouses;

public record GetWarehousesQuery(bool ActiveOnly = true, string? Search = null) : IQuery<IReadOnlyList<WarehouseDto>>;

public record WarehouseDto(
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? Address,
    string WarehouseType,
    string IntegrationSource,
    bool IsActive,
    DateTime CreatedAt);
