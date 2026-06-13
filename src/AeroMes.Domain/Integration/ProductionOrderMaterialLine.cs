using AeroMes.Domain.Common;

namespace AeroMes.Domain.Integration;

public class ProductionOrderMaterialLine : Entity
{
    public int LineId { get; private set; }
    public int POID { get; private set; }
    public string MaterialCode { get; private set; } = string.Empty;
    public decimal StandardQty { get; private set; }
    public decimal ActualQty { get; private set; }
    public string Unit { get; private set; } = string.Empty;

    private ProductionOrderMaterialLine() { }

    internal static ProductionOrderMaterialLine Create(
        int poid, string materialCode, decimal standardQty, string unit)
        => new()
        {
            POID = poid,
            MaterialCode = materialCode.Trim().ToUpperInvariant(),
            StandardQty = standardQty,
            ActualQty = standardQty,
            Unit = unit.Trim(),
        };

    public void AdjustActualQty(decimal actualQty) => ActualQty = actualQty;
}
