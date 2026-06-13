using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class MaterialTransferLine : Entity
{
    public int LineId { get; private set; }
    public int SlipId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string? SpecificationCode { get; private set; }

    private MaterialTransferLine() { }

    internal static MaterialTransferLine Create(
        int slipId,
        string productCode,
        string unitOfMeasure,
        decimal quantity,
        string? specificationCode)
    {
        return new MaterialTransferLine
        {
            SlipId = slipId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            Quantity = quantity,
            SpecificationCode = specificationCode?.Trim(),
        };
    }
}
