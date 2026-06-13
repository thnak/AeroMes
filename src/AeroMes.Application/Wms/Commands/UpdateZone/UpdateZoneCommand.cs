using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateZone;

public record UpdateZoneCommand(
    int ZoneId,
    string ZoneName,
    ZoneType ZoneType,
    TemperatureZone? TemperatureZone,
    string UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
