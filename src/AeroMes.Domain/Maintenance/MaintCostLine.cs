using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Maintenance;

public enum CostCategory { SparePart, Labor, Contractor, Consumable, Other }

public class MaintCostLine : Entity
{
    public int LineID { get; private set; }
    public int MaintOrderID { get; private set; }
    public CostCategory CostCategory { get; private set; }

    // spare part / consumable
    public string? ProductCode { get; private set; }
    public string? LotNumber { get; private set; }
    public decimal? QtyUsed { get; private set; }
    public decimal? UnitCost { get; private set; }

    // labor
    public string? OperatorID { get; private set; }
    public decimal? LaborHours { get; private set; }
    public decimal? LaborRateSnapshot { get; private set; }

    // contractor / other
    public int? SupplierID { get; private set; }
    public string? InvoiceRef { get; private set; }
    public decimal? InvoiceAmount { get; private set; }

    public decimal LineTotal { get; private set; }
    public string PostedBy { get; private set; } = string.Empty;
    public DateTime PostedAt { get; private set; }

    private MaintCostLine() { }

    public static MaintCostLine Create(
        int maintOrderId, CostCategory category,
        string? productCode, string? lotNumber, decimal? qtyUsed, decimal? unitCost,
        string? operatorId, decimal? laborHours, decimal? laborRateSnapshot,
        int? supplierId, string? invoiceRef, decimal? invoiceAmount,
        string postedBy)
    {
        var lineTotal = category switch
        {
            CostCategory.SparePart or CostCategory.Consumable => (qtyUsed ?? 0) * (unitCost ?? 0),
            CostCategory.Labor => (laborHours ?? 0) * (laborRateSnapshot ?? 0),
            _ => invoiceAmount ?? 0,
        };

        return new MaintCostLine
        {
            MaintOrderID = maintOrderId,
            CostCategory = category,
            ProductCode = productCode?.Trim().ToUpperInvariant(),
            LotNumber = lotNumber?.Trim(),
            QtyUsed = qtyUsed,
            UnitCost = unitCost,
            OperatorID = operatorId?.Trim(),
            LaborHours = laborHours,
            LaborRateSnapshot = laborRateSnapshot,
            SupplierID = supplierId,
            InvoiceRef = invoiceRef?.Trim(),
            InvoiceAmount = invoiceAmount,
            LineTotal = lineTotal,
            PostedBy = postedBy.Trim(),
            PostedAt = DateTime.UtcNow,
        };
    }
}
