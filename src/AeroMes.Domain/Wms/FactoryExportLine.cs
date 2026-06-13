using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class FactoryExportLine : Entity
{
    public int LineId { get; private set; }
    public int ExportId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public int SourceWarehouseId { get; private set; }
    public string? SpecificationCode { get; private set; }

    private FactoryExportLine() { }

    internal static FactoryExportLine Create(
        int exportId,
        string productCode,
        string unitOfMeasure,
        decimal quantity,
        int sourceWarehouseId,
        string? specificationCode)
    {
        return new FactoryExportLine
        {
            ExportId = exportId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            Quantity = quantity,
            SourceWarehouseId = sourceWarehouseId,
            SpecificationCode = specificationCode?.Trim(),
        };
    }
}
