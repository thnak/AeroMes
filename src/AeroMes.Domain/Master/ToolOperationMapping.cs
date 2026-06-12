using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

/// <summary>Which operations require this tool; ProductCode null = all products.</summary>
public class ToolOperationMapping : Entity
{
    public int MappingId { get; private set; }
    public int ToolId { get; private set; }
    public string OperationCode { get; private set; } = string.Empty;
    public string? ProductCode { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public decimal UsageCountPerOp { get; private set; } = 1m; // life units consumed per cycle

    // EF navigations
    public Operation? Operation { get; private set; }
    public Product? Product { get; private set; }

    private ToolOperationMapping() { }

    internal static ToolOperationMapping Create(
        int toolId, string operationCode, string? productCode, bool isRequired, decimal usageCountPerOp)
    {
        return new ToolOperationMapping
        {
            ToolId = toolId,
            OperationCode = operationCode,
            ProductCode = productCode,
            IsRequired = isRequired,
            UsageCountPerOp = usageCountPerOp,
        };
    }
}
