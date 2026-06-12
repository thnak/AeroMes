using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class ProductUoMConversion
{
    public int ConversionId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UoMCode { get; private set; } = string.Empty;
    public decimal ToBaseFactor { get; private set; } // 1 [UoMCode] = ToBaseFactor [BaseUoMCode]
    public string? Notes { get; private set; }

    private ProductUoMConversion() { }

    internal static ProductUoMConversion Create(string productCode, string uomCode, decimal toBaseFactor, string? notes)
    {
        EnsurePositive(toBaseFactor);
        return new ProductUoMConversion
        {
            ProductCode = productCode,
            UoMCode = uomCode,
            ToBaseFactor = toBaseFactor,
            Notes = notes?.Trim(),
        };
    }

    internal void Update(decimal toBaseFactor, string? notes)
    {
        EnsurePositive(toBaseFactor);
        ToBaseFactor = toBaseFactor;
        Notes = notes?.Trim();
    }

    private static void EnsurePositive(decimal factor)
    {
        if (factor <= 0)
            throw new DomainException("Tỷ lệ quy đổi phải lớn hơn 0.");
    }
}
