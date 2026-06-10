using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Product : AuditableEntity
{
    public string ProductCode { get; private set; } = string.Empty;   // PK — SKU
    public string ProductName { get; private set; } = string.Empty;
    public string ProductUnit { get; private set; } = string.Empty;   // e.g. Cái, Kg, Mét
    public string? BarcodePattern { get; private set; }               // regex for PDA validation
    public bool IsFinishedGood { get; private set; } = true;
    public bool IsActive { get; private set; } = true;

    private Product() { }

    public static Product Create(
        string code,
        string name,
        string unit,
        bool isFinishedGood = true,
        string? barcodePattern = null,
        string? createdBy = null)
    {
        return new Product
        {
            ProductCode = code.Trim().ToUpperInvariant(),
            ProductName = name.Trim(),
            ProductUnit = unit.Trim(),
            IsFinishedGood = isFinishedGood,
            BarcodePattern = barcodePattern,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, string unit, bool isFinishedGood, string? barcodePattern, string updatedBy)
    {
        ProductName = name.Trim();
        ProductUnit = unit.Trim();
        IsFinishedGood = isFinishedGood;
        BarcodePattern = barcodePattern;
        Touch(updatedBy);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
