using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.UpdateWarehouse;

public record UpdateWarehouseCommand(
    int WarehouseId,
    string Name,
    string? Address,
    WarehouseType WarehouseType,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
