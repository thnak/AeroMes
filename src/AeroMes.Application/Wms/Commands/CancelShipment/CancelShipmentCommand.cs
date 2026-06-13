using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CancelShipment;

public record CancelShipmentCommand(
    int ShipmentId,
    string? CancelledBy) : ICommand<ValidationResult<Unit>>;
