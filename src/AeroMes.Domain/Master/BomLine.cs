using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class BomLine : Entity
{
    public int BomLineId { get; private set; }
    public int BomHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public string ComponentCode { get; private set; } = string.Empty;
    public decimal RequiredQty { get; private set; }
    public string UoMCode { get; private set; } = string.Empty;
    public decimal ScrapFactor { get; private set; }   // allowable loss % (0–100)
    public bool IsPhantom { get; private set; }        // virtual assembly — skip in picking
    public string? Notes { get; private set; }

    // EF navigation
    public Product? Component { get; private set; }

    private BomLine() { }

    internal static BomLine Create(
        int bomHeaderId, int lineNo, string componentCode, decimal requiredQty,
        string uomCode, decimal scrapFactor, bool isPhantom, string? notes)
    {
        if (requiredQty <= 0)
            throw new DomainException($"Số lượng nguyên liệu phải lớn hơn 0. Dòng {lineNo}: {requiredQty}.");
        if (scrapFactor is < 0 or > 100)
            throw new DomainException($"Tỷ lệ hao hụt phải trong khoảng 0–100%. Dòng {lineNo}: {scrapFactor}.");

        return new BomLine
        {
            BomHeaderId = bomHeaderId,
            LineNo = lineNo,
            ComponentCode = componentCode.Trim().ToUpperInvariant(),
            RequiredQty = requiredQty,
            UoMCode = uomCode.Trim().ToUpperInvariant(),
            ScrapFactor = scrapFactor,
            IsPhantom = isPhantom,
            Notes = notes,
        };
    }
}
