using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ConfirmPickLine;

public record ConfirmPickLineCommand(
    long PickLineId,
    int PickListId,
    decimal ActualPickedQty,
    int? ScannedBinId,
    string? ScannedLotNumber,
    string? ConfirmedBy) : ICommand<ValidationResult<Unit>>;
