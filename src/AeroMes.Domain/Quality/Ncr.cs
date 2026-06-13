using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public class Ncr : Entity
{
    public int NcrId { get; private set; }
    public string NcrNo { get; private set; } = "";
    public int? InspectionOrderId { get; private set; }
    public long WorkOrderId { get; private set; }
    public string ProductCode { get; private set; } = "";
    public string? LotNumber { get; private set; }
    public decimal QtyAffected { get; private set; }
    public string Status { get; private set; } = "";  // OPEN | UNDER_REVIEW | DISPOSITION_SET | CLOSED | CANCELLED
    public string Severity { get; private set; } = "";  // CRITICAL | MAJOR | MINOR
    public string? DispositionCode { get; private set; }  // REWORK | SCRAP | USE_AS_IS | RETURN_TO_SUPPLIER | RE_INSPECT
    public string? DispositionSetBy { get; private set; }
    public DateTimeOffset? DispositionSetAt { get; private set; }
    public string? RootCause { get; private set; }
    public string? CorrectiveAction { get; private set; }
    public string? PreventiveAction { get; private set; }
    public string? AssignedTo { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public string? ClosedBy { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public string CreatedBy { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<NcrDefectLine> _defectLines = [];
    public IReadOnlyCollection<NcrDefectLine> DefectLines => _defectLines.AsReadOnly();

    private Ncr() { }

    public static Ncr Create(string ncrNo, int? inspectionOrderId, long workOrderId,
        string productCode, string? lotNumber, decimal qtyAffected,
        string severity, string createdBy)
    {
        var now = DateTimeOffset.UtcNow;
        return new Ncr
        {
            NcrNo = ncrNo,
            InspectionOrderId = inspectionOrderId,
            WorkOrderId = workOrderId,
            ProductCode = productCode,
            LotNumber = lotNumber,
            QtyAffected = qtyAffected,
            Status = "OPEN",
            Severity = severity,
            CreatedBy = createdBy,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void AddDefectLine(NcrDefectLine line)
    {
        _defectLines.Add(line);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Escalate()
    {
        if (Status != "OPEN")
            throw new DomainException($"Cannot escalate NCR in status {Status}.");
        Status = "UNDER_REVIEW";
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetDisposition(string dispositionCode, string setBy)
    {
        if (Status is not ("OPEN" or "UNDER_REVIEW"))
            throw new DomainException($"Cannot set disposition on NCR in status {Status}.");
        DispositionCode = dispositionCode;
        DispositionSetBy = setBy;
        DispositionSetAt = DateTimeOffset.UtcNow;
        Status = "DISPOSITION_SET";
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Close(string closedBy, string? rootCause, string? correctiveAction, string? preventiveAction)
    {
        if (Status != "DISPOSITION_SET")
            throw new DomainException($"NCR must have a disposition set before closing. Status: {Status}.");
        ClosedBy = closedBy;
        ClosedAt = DateTimeOffset.UtcNow;
        RootCause = rootCause;
        CorrectiveAction = correctiveAction;
        PreventiveAction = preventiveAction;
        Status = "CLOSED";
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(string cancelledBy)
    {
        if (Status is "CLOSED" or "CANCELLED")
            throw new DomainException($"Cannot cancel NCR in status {Status}.");
        Status = "CANCELLED";
        ClosedBy = cancelledBy;
        ClosedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string? assignedTo, DateTimeOffset? dueDate)
    {
        AssignedTo = assignedTo;
        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
