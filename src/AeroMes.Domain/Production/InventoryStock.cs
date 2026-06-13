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
    public decimal SecondaryQty { get; private set; }
    public decimal ReservedQty { get; private set; }
    public decimal AvailableQty => Quantity - ReservedQty;
    public DateTime UpdatedAt { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public DateOnly? ManufacturedDate { get; private set; }

    // EF navigations
    public StorageLocation? StorageLocation { get; private set; }

    private InventoryStock() { }

    public static InventoryStock Create(
        int locationId,
        string productCode,
        string lotNumber,
        decimal initialQuantity = 0m,
        decimal initialSecondaryQty = 0m)
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
            SecondaryQty = initialSecondaryQty,
            UpdatedAt = DateTime.UtcNow,
            ReceivedAt = DateTime.UtcNow,
        };
    }

    public void Adjust(decimal delta, decimal secondaryDelta = 0m)
    {
        var newQty = Quantity + delta;
        if (newQty < 0)
            throw new DomainException(
                $"Insufficient stock. Current: {Quantity}, requested delta: {delta}.");
        Quantity = newQty;
        SecondaryQty = Math.Max(0, SecondaryQty + secondaryDelta);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reserve(decimal qty)
    {
        if (qty <= 0)
            throw new DomainException("Reserve quantity must be positive.");
        var newReserved = ReservedQty + qty;
        if (newReserved > Quantity)
            throw new DomainException(
                $"Cannot reserve {qty}. Available: {AvailableQty}, reserved: {ReservedQty}, total: {Quantity}.");
        ReservedQty = newReserved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unreserve(decimal qty)
    {
        if (qty <= 0)
            throw new DomainException("Unreserve quantity must be positive.");
        ReservedQty = Math.Max(0, ReservedQty - qty);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBin(int? binId) => BinId = binId;

    public void SetLotDates(DateOnly? expiryDate, DateOnly? manufacturedDate)
    {
        ExpiryDate = expiryDate;
        ManufacturedDate = manufacturedDate;
    }
}
