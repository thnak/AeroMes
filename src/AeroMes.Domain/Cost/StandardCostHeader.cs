using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public enum StandardCostStatus { Draft, Approved, Active, Superseded }

public class StandardCostHeader : AuditableEntity
{
    public int StdCostId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int? BomHeaderId { get; private set; }
    public int? RoutingId { get; private set; }
    public int CostVersion { get; private set; }
    public StandardCostStatus Status { get; private set; } = StandardCostStatus.Draft;
    public decimal TotalMaterialCost { get; private set; }
    public decimal TotalLaborCost { get; private set; }
    public decimal TotalMachineCost { get; private set; }
    public decimal TotalOverheadCost { get; private set; }
    public decimal TotalStandardCost =>
        TotalMaterialCost + TotalLaborCost + TotalMachineCost + TotalOverheadCost;
    public string Currency { get; private set; } = "VND";
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime CalculatedAt { get; private set; }

    private readonly List<StandardCostMaterialLine> _materialLines = [];
    public IReadOnlyList<StandardCostMaterialLine> MaterialLines => _materialLines.AsReadOnly();

    private readonly List<StandardCostRoutingLine> _routingLines = [];
    public IReadOnlyList<StandardCostRoutingLine> RoutingLines => _routingLines.AsReadOnly();

    private StandardCostHeader() { }

    public static StandardCostHeader Create(
        string productCode, int? bomHeaderId, int? routingId,
        int costVersion, DateOnly effectiveFrom, string currency, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            throw new DomainException("Mã sản phẩm không được để trống.");

        return new StandardCostHeader
        {
            ProductCode = productCode.Trim().ToUpperInvariant(),
            BomHeaderId = bomHeaderId,
            RoutingId = routingId,
            CostVersion = costVersion,
            EffectiveFrom = effectiveFrom,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = StandardCostStatus.Draft,
            CalculatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void SetCosts(decimal material, decimal labor, decimal machine, decimal overhead)
    {
        TotalMaterialCost = material;
        TotalLaborCost = labor;
        TotalMachineCost = machine;
        TotalOverheadCost = overhead;
        CalculatedAt = DateTime.UtcNow;
    }

    public void AddMaterialLine(StandardCostMaterialLine line) => _materialLines.Add(line);
    public void AddRoutingLine(StandardCostRoutingLine line) => _routingLines.Add(line);

    public void Approve(string approvedBy)
    {
        if (Status != StandardCostStatus.Draft)
            throw new DomainException($"Chỉ có thể duyệt bảng giá ở trạng thái Draft. Hiện tại: {Status}.");
        Status = StandardCostStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Activate(DateOnly effectiveFrom)
    {
        if (Status != StandardCostStatus.Approved)
            throw new DomainException($"Chỉ có thể kích hoạt bảng giá ở trạng thái Approved. Hiện tại: {Status}.");
        Status = StandardCostStatus.Active;
        EffectiveFrom = effectiveFrom;
        Touch(ApprovedBy ?? CreatedBy ?? "system");
    }

    public void Supersede(DateOnly effectiveTo)
    {
        Status = StandardCostStatus.Superseded;
        EffectiveTo = effectiveTo;
    }
}
