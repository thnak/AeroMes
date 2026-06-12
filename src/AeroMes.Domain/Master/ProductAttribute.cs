using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class ProductAttribute : AuditableEntity
{
    public int AttributeId { get; private set; }
    public string AttributeCode { get; private set; } = string.Empty;
    public string AttributeName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private readonly List<ProductAttributeValue> _values = [];
    public IReadOnlyList<ProductAttributeValue> Values => _values.AsReadOnly();

    private ProductAttribute() { }

    public static ProductAttribute Create(string code, string name, string? createdBy)
    {
        return new ProductAttribute
        {
            AttributeCode = code.Trim().ToUpperInvariant(),
            AttributeName = name.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, bool isActive, string? updatedBy)
    {
        AttributeName = name.Trim();
        IsActive = isActive;
        Touch(updatedBy);
    }

    public ProductAttributeValue AddValue(string value, string? groupName, int sortOrder, string? updatedBy = null)
    {
        if (_values.Any(v => v.Value.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Giá trị '{value}' đã tồn tại trong thuộc tính '{AttributeCode}'.");

        var entry = ProductAttributeValue.Create(AttributeId, value, groupName, sortOrder);
        _values.Add(entry);
        Touch(updatedBy);
        return entry;
    }

    public void UpdateValue(int valueId, string value, string? groupName, int sortOrder, string? updatedBy = null)
    {
        var entry = _values.FirstOrDefault(v => v.ValueId == valueId)
            ?? throw new DomainException($"Không tìm thấy giá trị #{valueId} trong thuộc tính '{AttributeCode}'.");

        if (_values.Any(v => v.ValueId != valueId && v.Value.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Giá trị '{value}' đã tồn tại trong thuộc tính '{AttributeCode}'.");

        entry.Update(value, groupName, sortOrder);
        Touch(updatedBy);
    }

    public void RemoveValue(int valueId, string? updatedBy = null)
    {
        var entry = _values.FirstOrDefault(v => v.ValueId == valueId)
            ?? throw new DomainException($"Không tìm thấy giá trị #{valueId} trong thuộc tính '{AttributeCode}'.");
        _values.Remove(entry);
        Touch(updatedBy);
    }
}
