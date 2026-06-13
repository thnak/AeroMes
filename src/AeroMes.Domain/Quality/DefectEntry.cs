namespace AeroMes.Domain.Quality;

public class DefectEntry
{
    public int DefectEntryId { get; private set; }
    public long WorkOrderId { get; private set; }
    public int? JobId { get; private set; }
    public int DefectCodeId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal RepairableQty { get; private set; }
    public decimal ScrapQty { get; private set; }
    public string Status { get; private set; } = "PENDING";  // PENDING | IN_REPAIR | REPAIRED | SCRAPPED
    public string? Notes { get; private set; }
    public string CreatedBy { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }

    public DefectCode? DefectCode { get; private set; }

    private DefectEntry() { }

    public static DefectEntry Create(long workOrderId, int? jobId, int defectCodeId,
        decimal quantity, bool isRepairable, string createdBy, string? notes = null)
    {
        var repairableQty = isRepairable ? quantity : 0m;
        var scrapQty = isRepairable ? 0m : quantity;
        return new DefectEntry
        {
            WorkOrderId = workOrderId,
            JobId = jobId,
            DefectCodeId = defectCodeId,
            Quantity = quantity,
            RepairableQty = repairableQty,
            ScrapQty = scrapQty,
            Status = "PENDING",
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void MarkInRepair() => Status = "IN_REPAIR";
    public void MarkRepaired() => Status = "REPAIRED";
    public void MarkScrapped() => Status = "SCRAPPED";
}
