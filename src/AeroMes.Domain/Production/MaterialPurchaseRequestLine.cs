using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production;

public class MaterialPurchaseRequestLine : Entity
{
    public int LineID { get; private set; }
    public int RequestID { get; private set; }
    public string MaterialCode { get; private set; } = string.Empty;
    public string MaterialName { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal RequiredQty { get; private set; }
    public decimal? CalculatedQty { get; private set; }
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }
    public decimal? Radius { get; private set; }
    public decimal? Weight { get; private set; }

    private MaterialPurchaseRequestLine() { }

    internal static MaterialPurchaseRequestLine Create(
        int requestId, string materialCode, string materialName,
        string unitOfMeasure, decimal requiredQty)
    {
        return new MaterialPurchaseRequestLine
        {
            RequestID = requestId,
            MaterialCode = materialCode.Trim().ToUpperInvariant(),
            MaterialName = materialName.Trim(),
            UnitOfMeasure = unitOfMeasure.Trim().ToUpperInvariant(),
            RequiredQty = requiredQty,
        };
    }

    public void SetDimensions(
        decimal? length, decimal? width, decimal? height,
        decimal? radius, decimal? weight)
    {
        Length = length;
        Width = width;
        Height = height;
        Radius = radius;
        Weight = weight;
        RecalculateQty();
    }

    private void RecalculateQty()
    {
        if (Length.HasValue && Width.HasValue && Height.HasValue)
            CalculatedQty = Length.Value * Width.Value * Height.Value;
        else if (Radius.HasValue && Height.HasValue)
            CalculatedQty = 3.14159265358979m * Radius.Value * Radius.Value * Height.Value;
        else if (Weight.HasValue)
            CalculatedQty = Weight.Value;
        else
            CalculatedQty = null;
    }
}
