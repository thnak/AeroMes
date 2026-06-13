using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production;

public class FabricConsumptionLog : Entity
{
    public long ConsumptionID { get; private set; }
    public int RollID { get; private set; }
    public int CutOrderID { get; private set; }
    public decimal MetersConsumed { get; private set; }
    public decimal RemainingAfter { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public string RecordedBy { get; private set; } = string.Empty;

    private FabricConsumptionLog() { }

    public static FabricConsumptionLog Create(
        int rollId, int cutOrderId, decimal metersConsumed, decimal remainingAfter, string recordedBy)
        => new()
        {
            RollID = rollId,
            CutOrderID = cutOrderId,
            MetersConsumed = metersConsumed,
            RemainingAfter = remainingAfter,
            RecordedAt = DateTime.UtcNow,
            RecordedBy = recordedBy.Trim(),
        };
}
