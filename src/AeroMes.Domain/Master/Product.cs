using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Product : AuditableEntity
{
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? BarcodePattern { get; private set; }

    // Classification
    public ItemType ItemType { get; private set; } = ItemType.FG;
    public int? CategoryId { get; private set; }

    // Units of measure
    public string BaseUoMCode { get; private set; } = string.Empty;
    public string? PurchaseUoMCode { get; private set; }
    public decimal PurchaseToBaseQty { get; private set; } = 1m;

    // Physical attributes (mm / kg)
    public decimal? NetWeight { get; private set; }
    public decimal? GrossWeight { get; private set; }
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }

    // Lot / serial / shelf-life
    public bool LotControlled { get; private set; }
    public bool SerialControlled { get; private set; }
    public int? ShelfLifeDays { get; private set; }

    // Inventory policy
    public decimal? ReorderPoint { get; private set; }
    public decimal? SafetyStock { get; private set; }
    public int? LeadTimeDays { get; private set; }
    public ProcurementType? ProcurementType { get; private set; }

    // Lifecycle
    public LifecycleStatus LifecycleStatus { get; private set; } = LifecycleStatus.Active;
    public DateOnly? EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    // Identification
    public string? CustomerPartNo { get; private set; }
    public string? DrawingNo { get; private set; }
    public string? Revision { get; private set; }

    // Media
    public string? ImageUrl { get; private set; }
    public string? ThumbnailUrl { get; private set; }

    public bool IsActive { get; private set; } = true;

    // Navigation
    public ProductCategory? Category { get; private set; }

    private Product() { }

    public static Product Create(
        string code,
        string name,
        string baseUoMCode,
        ItemType itemType,
        int? categoryId,
        string? barcodePattern,
        bool lotControlled,
        bool serialControlled,
        int? shelfLifeDays,
        ProcurementType? procurementType,
        string? customerPartNo,
        string? drawingNo,
        string? revision,
        string? createdBy)
    {
        return new Product
        {
            ProductCode = code.Trim().ToUpperInvariant(),
            ProductName = name.Trim(),
            BaseUoMCode = baseUoMCode.Trim().ToUpperInvariant(),
            ItemType = itemType,
            CategoryId = categoryId,
            BarcodePattern = barcodePattern,
            LotControlled = lotControlled,
            SerialControlled = serialControlled,
            ShelfLifeDays = shelfLifeDays,
            ProcurementType = procurementType,
            CustomerPartNo = customerPartNo,
            DrawingNo = drawingNo,
            Revision = revision,
            LifecycleStatus = LifecycleStatus.Active,
            PurchaseToBaseQty = 1m,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string name,
        string baseUoMCode,
        string? purchaseUoMCode,
        decimal purchaseToBaseQty,
        ItemType itemType,
        int? categoryId,
        string? barcodePattern,
        bool lotControlled,
        bool serialControlled,
        int? shelfLifeDays,
        decimal? reorderPoint,
        decimal? safetyStock,
        int? leadTimeDays,
        ProcurementType? procurementType,
        DateOnly? effectiveFrom,
        DateOnly? effectiveTo,
        string? customerPartNo,
        string? drawingNo,
        string? revision,
        decimal? netWeight,
        decimal? grossWeight,
        decimal? length,
        decimal? width,
        decimal? height,
        string? imageUrl,
        string? thumbnailUrl,
        string updatedBy)
    {
        ProductName = name.Trim();
        BaseUoMCode = baseUoMCode.Trim().ToUpperInvariant();
        PurchaseUoMCode = purchaseUoMCode?.Trim().ToUpperInvariant();
        PurchaseToBaseQty = purchaseToBaseQty;
        ItemType = itemType;
        CategoryId = categoryId;
        BarcodePattern = barcodePattern;
        LotControlled = lotControlled;
        SerialControlled = serialControlled;
        ShelfLifeDays = shelfLifeDays;
        ReorderPoint = reorderPoint;
        SafetyStock = safetyStock;
        LeadTimeDays = leadTimeDays;
        ProcurementType = procurementType;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        CustomerPartNo = customerPartNo;
        DrawingNo = drawingNo;
        Revision = revision;
        NetWeight = netWeight;
        GrossWeight = grossWeight;
        Length = length;
        Width = width;
        Height = height;
        ImageUrl = imageUrl;
        ThumbnailUrl = thumbnailUrl;
        Touch(updatedBy);
    }

    public void ChangeLifecycleStatus(LifecycleStatus status, string updatedBy)
    {
        LifecycleStatus = status;
        IsActive = status == LifecycleStatus.Active;
        Touch(updatedBy);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
