using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.ShipSerial;

public record ShipSerialCommand(
    string SerialNumber,
    string ShipmentRef
) : ICommand<ValidationResult<Unit>>;
