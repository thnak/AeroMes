using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateZone;

public record CreateZoneCommand(
    string ZoneCode,
    string ZoneName,
    ZoneType ZoneType,
    int StorageLocationId,
    int? WarehouseId,
    TemperatureZone? TemperatureZone,
    string? CreatedBy)
    : ICommand<ValidationResult<int>>;
