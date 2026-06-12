using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ProductAttributeAssignment : AuditableEntity
{
    public int AssignmentId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int AttributeId { get; private set; }
    public int? SelectedValueId { get; private set; }

    public ProductAttribute? Attribute { get; private set; }
    public ProductAttributeValue? SelectedValue { get; private set; }

    private ProductAttributeAssignment() { }

    public static ProductAttributeAssignment Create(string productCode, int attributeId, int? selectedValueId, string? createdBy)
    {
        return new ProductAttributeAssignment
        {
            ProductCode = productCode.Trim().ToUpperInvariant(),
            AttributeId = attributeId,
            SelectedValueId = selectedValueId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void SelectValue(int? selectedValueId, string? updatedBy)
    {
        SelectedValueId = selectedValueId;
        Touch(updatedBy);
    }
}
