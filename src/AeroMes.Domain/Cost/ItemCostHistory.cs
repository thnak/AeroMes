using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public enum ItemCostType { STANDARD, MOVING_AVG, LAST_PO }

public class ItemCostHistory : Entity
{
    public int CostID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public ItemCostType CostType { get; private set; }
    public decimal UnitCost { get; private set; }
    public string CostUoM { get; private set; } = string.Empty;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string? SourceRef { get; private set; }
    public string? ApprovedBy { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ItemCostHistory() { }

    public static ItemCostHistory Create(
        string productCode, ItemCostType costType, decimal unitCost,
        string costUoM, DateOnly effectiveFrom,
        string? sourceRef, string? approvedBy, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(productCode)) throw new DomainException("Mã sản phẩm không được để trống.");
        if (unitCost <= 0) throw new DomainException("Đơn giá phải lớn hơn 0.");
        if (string.IsNullOrWhiteSpace(costUoM)) throw new DomainException("Đơn vị tính không được để trống.");
        return new ItemCostHistory
        {
            ProductCode = productCode.Trim(), CostType = costType,
            UnitCost = unitCost, CostUoM = costUoM, EffectiveFrom = effectiveFrom,
            SourceRef = sourceRef, ApprovedBy = approvedBy, CreatedBy = createdBy
        };
    }

    public void ExpireOn(DateOnly expiryDate) => EffectiveTo = expiryDate;
}
