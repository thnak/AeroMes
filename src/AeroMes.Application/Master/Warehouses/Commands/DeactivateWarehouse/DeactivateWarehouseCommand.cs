using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.DeactivateWarehouse;

public record DeactivateWarehouseCommand(int WarehouseId, string UpdatedBy) : ICommand<ValidationResult<Unit>>;
