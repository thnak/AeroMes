using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Commands.CreateWarehouse;

public record CreateWarehouseCommand(
    string Code,
    string Name,
    WarehouseType WarehouseType,
    string? Address,
    IntegrationSource IntegrationSource,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
