using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.DeleteWarehouse;

public record DeleteWarehouseCommand(int WarehouseId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
