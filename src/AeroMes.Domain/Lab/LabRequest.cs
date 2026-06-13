namespace AeroMes.Domain.Lab;

public class LabRequest
{
    public int RequestId { get; private set; }
    public string RequestNo { get; private set; } = string.Empty;
    public string RequestType { get; private set; } = string.Empty;
    public string Status { get; private set; } = "PENDING";
    public string Priority { get; private set; } = "ROUTINE";
    public string ProductCode { get; private set; } = string.Empty;
    public string? LotNumber { get; private set; }
    public long? WorkOrderId { get; private set; }
    public int? InspectionOrderId { get; private set; }
    public string? CustomerCode { get; private set; }
    public int? PanelId { get; private set; }
    public decimal SampleQty { get; private set; }
    public string SampleUnit { get; private set; } = string.Empty;
    public string? SampleLocation { get; private set; }
    public DateTimeOffset? RequiredBy { get; private set; }
    public string RequestedBy { get; private set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; private set; }
    public string? AssignedTo { get; private set; }
    public string? Notes { get; private set; }

    private LabRequest() { }

    public static LabRequest Create(string requestNo, string requestType, string priority,
        string productCode, string? lotNumber, long? workOrderId, int? inspectionOrderId,
        string? customerCode, int? panelId, decimal sampleQty, string sampleUnit,
        string? sampleLocation, DateTimeOffset? requiredBy, string requestedBy, string? notes)
        => new()
        {
            RequestNo = requestNo, RequestType = requestType, Status = "PENDING",
            Priority = priority, ProductCode = productCode, LotNumber = lotNumber,
            WorkOrderId = workOrderId, InspectionOrderId = inspectionOrderId,
            CustomerCode = customerCode, PanelId = panelId, SampleQty = sampleQty,
            SampleUnit = sampleUnit, SampleLocation = sampleLocation, RequiredBy = requiredBy,
            RequestedBy = requestedBy, RequestedAt = DateTimeOffset.UtcNow, Notes = notes,
        };

    public void Assign(string technicianName) { AssignedTo = technicianName; Status = "IN_PROGRESS"; }
    public void Cancel() { Status = "CANCELLED"; }
    public void Complete() { Status = "COMPLETED"; }
    public void BeginSampling() { Status = "SAMPLING"; }
}
