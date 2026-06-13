using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class Product : AuditableEntity
{
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? BarcodePattern { get; private set; }

    // Classification
    public ItemType ItemType { get; private set; } = ItemType.FG;
    public int? CategoryId { get; private set; }

    // Variant model (MaterialManagementType = VariantCode): variants are full
    // products linked to their master via ParentProductCode.
    public string? ParentProductCode { get; private set; }

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

    // Procurement / technical (Materials & Goods item master)
    public decimal? FixedPurchasePrice { get; private set; }
    public string? TechnicalStandard { get; private set; }
    public string? QuantityFormula { get; private set; } // bracketed dimension variables, e.g. [Height]*[Width]*[Qty]

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

    // Lot allocation
    public PickingStrategy PickingStrategy { get; private set; } = PickingStrategy.Fefo;
    public int? MinShelfLifeDaysOnIssue { get; private set; }

    // Navigation
    public ProductCategory? Category { get; private set; }

    private readonly List<ProductUoMConversion> _uomConversions = [];
    public IReadOnlyList<ProductUoMConversion> UoMConversions => _uomConversions.AsReadOnly();

    // Specification model (MaterialManagementType = SpecificationCode):
    // technical variants share this master code via supplemental spec codes.
    private readonly List<ProductSpecification> _specifications = [];
    public IReadOnlyList<ProductSpecification> Specifications => _specifications.AsReadOnly();

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
        decimal? fixedPurchasePrice,
        string? technicalStandard,
        string? quantityFormula,
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
        FixedPurchasePrice = fixedPurchasePrice;
        TechnicalStandard = technicalStandard?.Trim();
        QuantityFormula = quantityFormula?.Trim();
        Touch(updatedBy);
    }

    public ProductUoMConversion AddUoMConversion(string uomCode, decimal toBaseFactor, string? notes, string? updatedBy = null)
    {
        var normalized = uomCode.Trim().ToUpperInvariant();
        if (normalized == BaseUoMCode)
            throw new DomainException($"Không thể khai báo quy đổi cho chính đơn vị gốc '{BaseUoMCode}'.");
        if (_uomConversions.Any(c => c.UoMCode == normalized))
            throw new DomainException($"Quy đổi sang đơn vị '{normalized}' đã tồn tại cho sản phẩm '{ProductCode}'.");

        var conversion = ProductUoMConversion.Create(ProductCode, normalized, toBaseFactor, notes);
        _uomConversions.Add(conversion);
        Touch(updatedBy);
        return conversion;
    }

    public void UpdateUoMConversion(int conversionId, decimal toBaseFactor, string? notes, string? updatedBy = null)
    {
        var conversion = _uomConversions.FirstOrDefault(c => c.ConversionId == conversionId)
            ?? throw new DomainException($"Không tìm thấy quy đổi #{conversionId} của sản phẩm '{ProductCode}'.");
        conversion.Update(toBaseFactor, notes);
        Touch(updatedBy);
    }

    public void RemoveUoMConversion(int conversionId, string? updatedBy = null)
    {
        var conversion = _uomConversions.FirstOrDefault(c => c.ConversionId == conversionId)
            ?? throw new DomainException($"Không tìm thấy quy đổi #{conversionId} của sản phẩm '{ProductCode}'.");
        _uomConversions.Remove(conversion);
        Touch(updatedBy);
    }

    public void SetVariantParent(string parentProductCode)
    {
        var normalized = parentProductCode.Trim().ToUpperInvariant();
        if (normalized == ProductCode)
            throw new DomainException("Biến thể không thể là sản phẩm gốc của chính nó.");
        ParentProductCode = normalized;
    }

    public ProductSpecification AddSpecification(string specCode, string? description, string? updatedBy = null)
    {
        var normalized = specCode.Trim().ToUpperInvariant();
        if (_specifications.Any(s => s.SpecCode == normalized))
            throw new DomainException($"Mã quy cách '{normalized}' đã tồn tại cho sản phẩm '{ProductCode}'.");

        var spec = ProductSpecification.Create(ProductCode, normalized, description);
        _specifications.Add(spec);
        Touch(updatedBy);
        return spec;
    }

    public void UpdateSpecification(int specificationId, string? description, bool isActive, string? updatedBy = null)
    {
        var spec = _specifications.FirstOrDefault(s => s.SpecificationId == specificationId)
            ?? throw new DomainException($"Không tìm thấy mã quy cách #{specificationId} của sản phẩm '{ProductCode}'.");
        spec.Update(description, isActive);
        Touch(updatedBy);
    }

    public void RemoveSpecification(int specificationId, string? updatedBy = null)
    {
        var spec = _specifications.FirstOrDefault(s => s.SpecificationId == specificationId)
            ?? throw new DomainException($"Không tìm thấy mã quy cách #{specificationId} của sản phẩm '{ProductCode}'.");
        _specifications.Remove(spec);
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

    public void UpdatePickingConfig(PickingStrategy strategy, int? minShelfLifeDays, string updatedBy)
    {
        if (minShelfLifeDays.HasValue && minShelfLifeDays.Value < 0)
            throw new DomainException("Số ngày hạn dùng tối thiểu không được âm.");
        PickingStrategy = strategy;
        MinShelfLifeDaysOnIssue = minShelfLifeDays;
        Touch(updatedBy);
    }
}
