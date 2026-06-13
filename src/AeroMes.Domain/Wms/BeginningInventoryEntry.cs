using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;

namespace AeroMes.Domain.Wms;

public class BeginningInventoryEntry : AuditableEntity
{
    public int EntryId { get; private set; }
    public DateOnly Period { get; private set; }
    public int WarehouseId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal BeginningQuantity { get; private set; }
    public string? LotNumber { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }

    // EF navigation
    public Warehouse? Warehouse { get; private set; }

    private BeginningInventoryEntry() { }

    public static BeginningInventoryEntry Create(
        DateOnly period,
        int warehouseId,
        string productCode,
        string unitOfMeasure,
        decimal beginningQuantity,
        string? lotNumber,
        DateOnly? expirationDate,
        string? createdBy)
    {
        if (beginningQuantity < 0)
            throw new DomainException("Số lượng tồn đầu kỳ không được âm.");

        return new BeginningInventoryEntry
        {
            Period = period,
            WarehouseId = warehouseId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            BeginningQuantity = beginningQuantity,
            LotNumber = lotNumber?.Trim(),
            ExpirationDate = expirationDate,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        decimal beginningQuantity,
        string? lotNumber,
        DateOnly? expirationDate,
        string? updatedBy)
    {
        if (beginningQuantity < 0)
            throw new DomainException("Số lượng tồn đầu kỳ không được âm.");

        BeginningQuantity = beginningQuantity;
        LotNumber = lotNumber?.Trim();
        ExpirationDate = expirationDate;
        Touch(updatedBy);
    }
}
