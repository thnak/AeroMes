using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class StockPolicy : AuditableEntity
{
    public int PolicyId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int LocationId { get; private set; }
    public decimal MinQty { get; private set; }
    public decimal MaxQty { get; private set; }
    public decimal SafetyStockQty { get; private set; }
    public decimal ReorderQty { get; private set; }
    public int LeadTimeDays { get; private set; }
    public bool IsActive { get; private set; } = true;

    private StockPolicy() { }

    public static StockPolicy Create(
        string productCode,
        int locationId,
        decimal minQty,
        decimal maxQty,
        decimal safetyStockQty,
        decimal reorderQty,
        int leadTimeDays,
        string? createdBy)
    {
        if (minQty < 0)
            throw new DomainException("Mức tồn kho tối thiểu không được âm.");
        if (maxQty <= minQty)
            throw new DomainException("Mức tồn kho tối đa phải lớn hơn mức tối thiểu.");
        if (safetyStockQty < 0 || safetyStockQty > minQty)
            throw new DomainException("Tồn kho an toàn phải trong khoảng [0, MinQty].");
        if (reorderQty <= 0)
            throw new DomainException("Số lượng đặt hàng phải lớn hơn 0.");

        return new StockPolicy
        {
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LocationId = locationId,
            MinQty = minQty,
            MaxQty = maxQty,
            SafetyStockQty = safetyStockQty,
            ReorderQty = reorderQty,
            LeadTimeDays = leadTimeDays < 0 ? 0 : leadTimeDays,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        decimal minQty,
        decimal maxQty,
        decimal safetyStockQty,
        decimal reorderQty,
        int leadTimeDays,
        bool isActive,
        string? updatedBy)
    {
        if (minQty < 0)
            throw new DomainException("Mức tồn kho tối thiểu không được âm.");
        if (maxQty <= minQty)
            throw new DomainException("Mức tồn kho tối đa phải lớn hơn mức tối thiểu.");
        if (safetyStockQty < 0 || safetyStockQty > minQty)
            throw new DomainException("Tồn kho an toàn phải trong khoảng [0, MinQty].");
        if (reorderQty <= 0)
            throw new DomainException("Số lượng đặt hàng phải lớn hơn 0.");

        MinQty = minQty;
        MaxQty = maxQty;
        SafetyStockQty = safetyStockQty;
        ReorderQty = reorderQty;
        LeadTimeDays = leadTimeDays < 0 ? 0 : leadTimeDays;
        IsActive = isActive;
        Touch(updatedBy);
    }
}
