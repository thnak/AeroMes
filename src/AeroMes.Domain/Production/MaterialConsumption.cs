using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public class MaterialConsumption : Entity
{
    public long ConsumptionId { get; private set; }
    public long JobId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string? LotNumber { get; private set; }
    public decimal PlannedQty { get; private set; }
    public decimal ActualQty { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public string? IssuedBy { get; private set; }
    public int? LocationId { get; private set; }

    private MaterialConsumption() { }

    public static MaterialConsumption CreatePlanned(
        long jobId, string productCode, decimal plannedQty)
        => new()
        {
            JobId = jobId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            PlannedQty = plannedQty,
            ActualQty = 0,
        };

    public void Confirm(string lotNumber, decimal actualQty, string issuedBy, int locationId)
    {
        if (actualQty < 0)
            throw new DomainException("Số lượng thực tế không thể âm.");
        LotNumber = lotNumber.Trim();
        ActualQty = actualQty;
        IssuedBy = issuedBy.Trim();
        LocationId = locationId;
        IssuedAt = DateTime.UtcNow;
    }
}
