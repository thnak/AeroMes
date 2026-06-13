using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DispatchShipment;

public record DispatchShipmentCommand(
    int ShipmentId,
    string? CarrierName,
    string? TrackingNumber,
    string? DispatchedBy) : ICommand<ValidationResult<Unit>>;
