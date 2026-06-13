using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteZone;

public record DeleteZoneCommand(int ZoneId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
