namespace AeroMes.Domain.Exceptions;

public class LotUnderActiveHoldException(
    string lotNumber,
    Guid holdId,
    string holdReason,
    string? holdReference)
    : Exception($"Lot {lotNumber} is under active hold ({holdReason}). Consumption blocked.")
{
    public string LotNumber { get; } = lotNumber;
    public Guid HoldID { get; } = holdId;
    public string HoldReason { get; } = holdReason;
    public string? HoldReference { get; } = holdReference;
}
