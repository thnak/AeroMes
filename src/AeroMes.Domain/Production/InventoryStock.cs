using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;

namespace AeroMes.Domain.Production;

public class InventoryStock : Entity
{
    public long StockID { get; private set; }
    public int LocationID { get; private set; }
    public int? BinId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF navigations
    public StorageLocation? StorageLocation { get; private set; }

    private InventoryStock() { }

    public static InventoryStock Create(
        int locationId,
        string productCode,
        string lotNumber,
        decimal initialQuantity = 0m)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            throw new DomainException("Product code is required.");
        if (string.IsNullOrWhiteSpace(lotNumber))
            throw new DomainException("Lot number is required.");
        if (initialQuantity < 0)
            throw new DomainException($"Initial quantity cannot be negative. Got: {initialQuantity}.");

        return new InventoryStock
        {
            LocationID = locationId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            Quantity = initialQuantity,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Adjust(decimal delta)
    {
        var newQty = Quantity + delta;
        if (newQty < 0)
            throw new DomainException(
                $"Insufficient stock. Current: {Quantity}, requested delta: {delta}.");
        Quantity = newQty;
        UpdatedAt = DateTime.UtcNow;
    }
}
