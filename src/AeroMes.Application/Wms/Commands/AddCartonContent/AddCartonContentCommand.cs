using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AddCartonContent;

public record AddCartonContentCommand(
    int CartonId,
    string ProductCode,
    string LotNumber,
    decimal PackedQty,
    string? CreatedBy) : ICommand<ValidationResult<Unit>>;
