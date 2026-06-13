using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production;

public class CutOrderFabricUsage : Entity
{
    public int UsageID { get; private set; }
    public int CutOrderID { get; private set; }
    public int RollID { get; private set; }
    public decimal MetersUsed { get; private set; }

    private CutOrderFabricUsage() { }

    public static CutOrderFabricUsage Create(int rollId)
        => new() { RollID = rollId };

    public void UpdateMetersUsed(decimal meters) => MetersUsed = meters;
}
