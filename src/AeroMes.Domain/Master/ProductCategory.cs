using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ProductCategory : AuditableEntity
{
    public int CategoryId { get; private set; }
    public int? ParentId { get; private set; }
    public string CategoryCode { get; private set; } = string.Empty;
    public string CategoryName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Production-scheduling metadata (used when planning production by purchase order)
    public decimal? StandardProductionTime { get; private set; } // hours
    public string? Color { get; private set; }                   // display color (hex)

    public bool IsActive { get; private set; } = true;

    public ProductCategory? Parent { get; private set; }

    private readonly List<ProductCategory> _children = [];
    public IReadOnlyList<ProductCategory> Children => _children.AsReadOnly();

    private ProductCategory() { }

    public static ProductCategory Create(
        int? parentId,
        string code,
        string name,
        string? description = null,
        decimal? standardProductionTime = null,
        string? color = null,
        string? createdBy = null)
    {
        return new ProductCategory
        {
            ParentId = parentId,
            CategoryCode = code.Trim().ToUpperInvariant(),
            CategoryName = name.Trim(),
            Description = description?.Trim(),
            StandardProductionTime = standardProductionTime,
            Color = color?.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        int? parentId,
        string name,
        string? description,
        decimal? standardProductionTime,
        string? color,
        bool isActive,
        string updatedBy)
    {
        ParentId = parentId;
        CategoryName = name.Trim();
        Description = description?.Trim();
        StandardProductionTime = standardProductionTime;
        Color = color?.Trim();
        IsActive = isActive;
        Touch(updatedBy);
    }
}
