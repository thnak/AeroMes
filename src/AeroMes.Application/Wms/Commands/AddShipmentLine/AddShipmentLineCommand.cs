using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AddShipmentLine;

public record AddShipmentLineCommand(
    int ShipmentId,
    string ProductCode,
    decimal OrderedQty,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
