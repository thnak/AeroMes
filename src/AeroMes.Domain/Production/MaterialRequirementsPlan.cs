using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum MrpStatus { Draft, Calculated, Confirmed, Closed }

public class MaterialRequirementsPlan : AuditableEntity
{
    public int MrpID { get; private set; }
    public string PlanNumber { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public int? MasterPlanId { get; private set; }
    public string? OrganizationalUnit { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public MrpStatus Status { get; private set; } = MrpStatus.Draft;
    public string? Notes { get; private set; }

    private readonly List<MrpLine> _lines = [];
    public IReadOnlyList<MrpLine> Lines => _lines.AsReadOnly();

    private MaterialRequirementsPlan() { }

    public static MaterialRequirementsPlan Create(
        string planNumber, string planName, int? masterPlanId,
        string? orgUnit, DateOnly periodStart, DateOnly periodEnd,
        string? notes, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(planNumber)) throw new DomainException("Số kế hoạch không được để trống.");
        if (string.IsNullOrWhiteSpace(planName)) throw new DomainException("Tên kế hoạch không được để trống.");
        if (periodEnd < periodStart) throw new DomainException("Ngày kết thúc phải sau ngày bắt đầu kỳ kế hoạch.");
        return new MaterialRequirementsPlan
        {
            PlanNumber = planNumber.Trim().ToUpperInvariant(),
            PlanName = planName.Trim(),
            MasterPlanId = masterPlanId,
            OrganizationalUnit = orgUnit?.Trim(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Notes = notes?.Trim(),
            CreatedBy = createdBy
        };
    }

    public void Update(string planName, string? orgUnit, DateOnly periodStart, DateOnly periodEnd,
        string? notes, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(planName)) throw new DomainException("Tên kế hoạch không được để trống.");
        PlanName = planName.Trim();
        OrganizationalUnit = orgUnit?.Trim();
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void SetLines(IEnumerable<MrpLine> lines, string? updatedBy)
    {
        _lines.Clear();
        _lines.AddRange(lines);
        Touch(updatedBy);
    }

    public void MarkCalculated(string? updatedBy) { Status = MrpStatus.Calculated; Touch(updatedBy); }
    public void Confirm(string? updatedBy) { Status = MrpStatus.Confirmed; Touch(updatedBy); }
    public void Close(string? updatedBy) { Status = MrpStatus.Closed; Touch(updatedBy); }
}

public class MrpLine
{
    public int MrpLineID { get; private set; }
    public int MrpID { get; private set; }
    public string FinishedGoodCode { get; private set; } = string.Empty;
    public decimal FinishedGoodQty { get; private set; }
    public string MaterialCode { get; private set; } = string.Empty;
    public string MaterialName { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal FixedWaste { get; private set; }
    public decimal WasteRatio { get; private set; }
    public decimal CalculatedMaterialQty { get; private set; }
    public decimal OpeningInventory { get; private set; }
    public decimal ConcurrentPurchaseRequestQty { get; private set; }
    public decimal PlannedOrderQty { get; private set; }
    public decimal ForecastedClosingBalance =>
        OpeningInventory + PlannedOrderQty + ConcurrentPurchaseRequestQty - CalculatedMaterialQty;
    public bool HasShortfall => ForecastedClosingBalance < 0;

    private MrpLine() { }

    public static MrpLine Create(
        int mrpId, string finishedGoodCode, decimal finishedGoodQty,
        string materialCode, string materialName, string uom,
        decimal fixedWaste, decimal wasteRatio,
        decimal openingInventory, decimal concurrentPurchaseRequestQty)
    {
        var calculatedQty = fixedWaste + finishedGoodQty * wasteRatio;
        return new MrpLine
        {
            MrpID = mrpId,
            FinishedGoodCode = finishedGoodCode.Trim(),
            FinishedGoodQty = finishedGoodQty,
            MaterialCode = materialCode.Trim(),
            MaterialName = materialName.Trim(),
            UnitOfMeasure = uom.Trim(),
            FixedWaste = fixedWaste,
            WasteRatio = wasteRatio,
            CalculatedMaterialQty = calculatedQty,
            OpeningInventory = openingInventory,
            ConcurrentPurchaseRequestQty = concurrentPurchaseRequestQty,
            PlannedOrderQty = Math.Max(0m, calculatedQty - openingInventory - concurrentPurchaseRequestQty)
        };
    }

    public void AdjustPlannedOrderQty(decimal qty)
    {
        PlannedOrderQty = qty >= 0 ? qty : throw new DomainException("Số lượng đặt hàng không được âm.");
    }

    public void OverrideCalculation(decimal fixedWaste, decimal wasteRatio, decimal finishedGoodQty)
    {
        FixedWaste = fixedWaste;
        WasteRatio = wasteRatio;
        FinishedGoodQty = finishedGoodQty;
        CalculatedMaterialQty = fixedWaste + finishedGoodQty * wasteRatio;
    }
}
