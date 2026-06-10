using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class BomItem : AuditableEntity
{
    public int BomID { get; private set; }
    public string ParentProductCode { get; private set; } = string.Empty;
    public string ChildProductCode { get; private set; } = string.Empty;
    public decimal RequiredQty { get; private set; }     // qty of child needed to make 1 parent
    public decimal ScrapFactor { get; private set; }     // allowable loss % (0–100)
    public bool IsActive { get; private set; } = true;

    private BomItem() { }

    public static BomItem Create(
        string parentCode,
        string childCode,
        decimal requiredQty,
        decimal scrapFactor = 0m,
        string? createdBy = null)
    {
        return new BomItem
        {
            ParentProductCode = parentCode.Trim().ToUpperInvariant(),
            ChildProductCode = childCode.Trim().ToUpperInvariant(),
            RequiredQty = requiredQty,
            ScrapFactor = scrapFactor,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateQty(decimal requiredQty, decimal scrapFactor, string updatedBy)
    {
        RequiredQty = requiredQty;
        ScrapFactor = scrapFactor;
        Touch(updatedBy);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
