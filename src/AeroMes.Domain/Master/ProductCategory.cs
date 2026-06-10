using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ProductCategory : AuditableEntity
{
    public int CategoryId { get; private set; }
    public int? ParentId { get; private set; }
    public string CategoryCode { get; private set; } = string.Empty;
    public string CategoryName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public ProductCategory? Parent { get; private set; }

    private readonly List<ProductCategory> _children = [];
    public IReadOnlyList<ProductCategory> Children => _children.AsReadOnly();

    private ProductCategory() { }

    public static ProductCategory Create(int? parentId, string code, string name, string? createdBy)
    {
        return new ProductCategory
        {
            ParentId = parentId,
            CategoryCode = code.Trim().ToUpperInvariant(),
            CategoryName = name.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(int? parentId, string name, bool isActive, string updatedBy)
    {
        ParentId = parentId;
        CategoryName = name.Trim();
        IsActive = isActive;
        Touch(updatedBy);
    }
}
