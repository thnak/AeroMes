using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ReceiveRma;

public record ReceiveRmaCommand(
    int RmaId,
    IReadOnlyList<RmaLineReceiptEntry> LineReceipts,
    int? QuarantineBinId,
    int QuarantineLocationId,
    string? ReceivedBy) : ICommand<ValidationResult<Unit>>;

public record RmaLineReceiptEntry(int LineId, decimal ReceivedQty);
