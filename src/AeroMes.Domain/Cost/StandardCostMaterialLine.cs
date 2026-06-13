using AeroMes.Domain.Common;

namespace AeroMes.Domain.Cost;

public class StandardCostMaterialLine : Entity
{
    public int LineId { get; private set; }
    public int StdCostId { get; private set; }
    public string ComponentCode { get; private set; } = string.Empty;
    public decimal RequiredQty { get; private set; }
    public decimal ScrapFactor { get; private set; }
    public decimal AdjustedQty => RequiredQty * (1m + ScrapFactor / 100m);
    public decimal UnitCost { get; private set; }
    public decimal LineTotal => AdjustedQty * UnitCost;

    private StandardCostMaterialLine() { }

    public static StandardCostMaterialLine Create(
        int stdCostId, string componentCode,
        decimal requiredQty, decimal scrapFactor, decimal unitCost)
        => new()
        {
            StdCostId = stdCostId,
            ComponentCode = componentCode.Trim().ToUpperInvariant(),
            RequiredQty = requiredQty,
            ScrapFactor = scrapFactor,
            UnitCost = unitCost,
        };
}
