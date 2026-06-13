using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ReceiveFinishedProductIntakeRequest;

public record ReceiveFinishedProductIntakeRequestCommand(
    int IntakeRequestId,
    IReadOnlyList<ActualReceiptLineInput> ReceiptLines,
    string? ReceivedBy
) : ICommand<ValidationResult<Unit>>;

public record ActualReceiptLineInput(int LineId, decimal ActualReceivedQuantity);
