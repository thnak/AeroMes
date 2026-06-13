using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public class CutOrderLine : Entity
{
    public int LineID { get; private set; }
    public int CutOrderID { get; private set; }
    public string SizeCode { get; private set; } = string.Empty;
    public int QuantityToCut { get; private set; }
    public int QuantityCut { get; private set; }

    private CutOrderLine() { }

    public static CutOrderLine Create(string sizeCode, int quantityToCut)
    {
        if (quantityToCut <= 0) throw new DomainException("Quantity to cut must be positive.");
        return new CutOrderLine
        {
            SizeCode = sizeCode.Trim().ToUpperInvariant(),
            QuantityToCut = quantityToCut,
        };
    }

    public void RecordCut(int qtyCut)
    {
        if (qtyCut < 0) throw new DomainException("Cut quantity cannot be negative.");
        QuantityCut = qtyCut;
    }
}
