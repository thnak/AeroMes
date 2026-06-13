using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.RecallSerial;

public record RecallSerialCommand(
    string SerialNumber,
    Guid RecallID
) : ICommand<ValidationResult<Unit>>;
