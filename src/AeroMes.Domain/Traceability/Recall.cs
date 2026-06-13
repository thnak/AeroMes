using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability.Events;

namespace AeroMes.Domain.Traceability;

public enum RecallType
{
    SupplierAlert, CustomerComplaint, InternalDetection,
    RegulatoryRequest, PreventiveAction
}

public enum RecallStatus
{
    Open, ScopeIdentified, QuarantineApplied, Investigating, Closed, Archived
}

public enum AnchorDirection { Backward, Forward, Bidirectional }

public enum LotCategory
{
    AnchorLot, RawMaterial, WIPInProcess, WIPComplete, FinishedGoods, Shipped, Unknown
}

public class Recall : Entity
{
    public Guid RecallID { get; private set; }
    public string RecallCode { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public RecallType RecallType { get; private set; }
    public RecallStatus Status { get; private set; } = RecallStatus.Open;
    public string AnchorLotNumber { get; private set; } = string.Empty;
    public AnchorDirection AnchorDirection { get; private set; }
    public string? Description { get; private set; }
    public string? RegulatoryRef { get; private set; }
    public string InitiatedBy { get; private set; } = string.Empty;
    public DateTime InitiatedAt { get; private set; }
    public DateTime? ScopeIdentifiedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? ClosedBy { get; private set; }

    private Recall() { }

    public static Recall Initiate(
        string recallCode,
        string title,
        RecallType recallType,
        string anchorLotNumber,
        AnchorDirection anchorDirection,
        string initiatedBy,
        string? description = null,
        string? regulatoryRef = null)
    {
        if (string.IsNullOrWhiteSpace(recallCode)) throw new DomainException("Recall code is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(anchorLotNumber)) throw new DomainException("Anchor lot number is required.");
        if (string.IsNullOrWhiteSpace(initiatedBy)) throw new DomainException("Initiated by is required.");

        var recall = new Recall
        {
            RecallID = Guid.NewGuid(),
            RecallCode = recallCode.Trim().ToUpperInvariant(),
            Title = title.Trim(),
            RecallType = recallType,
            Status = RecallStatus.Open,
            AnchorLotNumber = anchorLotNumber.Trim().ToUpperInvariant(),
            AnchorDirection = anchorDirection,
            Description = description?.Trim(),
            RegulatoryRef = regulatoryRef?.Trim(),
            InitiatedBy = initiatedBy.Trim(),
            InitiatedAt = DateTime.UtcNow,
        };

        recall.RaiseDomainEvent(new RecallInitiatedEvent(
            recall.RecallID, recall.RecallCode, recall.AnchorLotNumber, recallType.ToString()));

        return recall;
    }

    public void MarkScopeIdentified(int totalLots, int wipCount, int shippedCount)
    {
        if (Status != RecallStatus.Open)
            throw new DomainException("Scope can only be identified for Open recalls.");
        Status = RecallStatus.ScopeIdentified;
        ScopeIdentifiedAt = DateTime.UtcNow;
        RaiseDomainEvent(new RecallScopeIdentifiedEvent(RecallID, totalLots, wipCount, shippedCount));
    }

    public void MarkQuarantineApplied(int holdsPlaced, IReadOnlyList<string> affectedLots)
    {
        if (Status != RecallStatus.ScopeIdentified)
            throw new DomainException("Quarantine can only be applied after scope is identified.");
        Status = RecallStatus.QuarantineApplied;
        RaiseDomainEvent(new RecallQuarantineAppliedEvent(RecallID, holdsPlaced, affectedLots));
    }

    public void Close(string closedBy)
    {
        if (Status is RecallStatus.Closed or RecallStatus.Archived)
            throw new DomainException("Recall is already closed.");
        if (string.IsNullOrWhiteSpace(closedBy)) throw new DomainException("Closed by is required.");
        Status = RecallStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedBy = closedBy.Trim();
        RaiseDomainEvent(new RecallClosedEvent(RecallID, RecallCode, closedBy));
    }
}
