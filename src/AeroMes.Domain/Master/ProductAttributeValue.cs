namespace AeroMes.Domain.Master;

public class ProductAttributeValue
{
    public int ValueId { get; private set; }
    public int AttributeId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string? GroupName { get; private set; }
    public int SortOrder { get; private set; }

    private ProductAttributeValue() { }

    internal static ProductAttributeValue Create(int attributeId, string value, string? groupName, int sortOrder)
    {
        return new ProductAttributeValue
        {
            AttributeId = attributeId,
            Value = value.Trim(),
            GroupName = groupName?.Trim(),
            SortOrder = sortOrder,
        };
    }

    internal void Update(string value, string? groupName, int sortOrder)
    {
        Value = value.Trim();
        GroupName = groupName?.Trim();
        SortOrder = sortOrder;
    }
}
