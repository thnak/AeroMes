using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability.Events;

namespace AeroMes.Domain.Traceability;

public enum LotHoldStatus { Active, Released, Rejected, Scrapped, Transferred }

public enum HoldReason
{
    SupplierAlert, RecallInvestigation, FailedInspection, MissingCoA,
    SpecDeviation, ProcessExcursion, CustomerComplaint, RegulatoryRequest, PreventiveHold
}

public enum HoldDispositionCode
{
    UseAsIs, Rework, ReturnToSupplier, Destroy, ConditionalRelease, Downgrade
}

public class LotHold : Entity
{
    public Guid HoldID { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public string? ProductCode { get; private set; }
    public int? WorkOrderID { get; private set; }

    public LotHoldStatus HoldStatus { get; private set; } = LotHoldStatus.Active;
    public HoldReason HoldReason { get; private set; }
    public string? HoldDescription { get; private set; }
    public string? HoldReference { get; private set; }

    public string HoldInitiatedBy { get; private set; } = string.Empty;
    public DateTime HoldInitiatedAt { get; private set; }

    public HoldDispositionCode? DispositionCode { get; private set; }
    public string? DispositionNotes { get; private set; }
    public string? ReleasedBy { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public string? ESignatureRef { get; private set; }

    private LotHold() { }

    public static LotHold Place(
        string lotNumber,
        HoldReason holdReason,
        string initiatedBy,
        string? productCode = null,
        int? workOrderId = null,
        string? holdDescription = null,
        string? holdReference = null)
    {
        if (string.IsNullOrWhiteSpace(lotNumber)) throw new DomainException("Lot number is required.");
        if (string.IsNullOrWhiteSpace(initiatedBy)) throw new DomainException("Initiated by is required.");

        var hold = new LotHold
        {
            HoldID = Guid.NewGuid(),
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            ProductCode = productCode?.Trim().ToUpperInvariant(),
            WorkOrderID = workOrderId,
            HoldStatus = LotHoldStatus.Active,
            HoldReason = holdReason,
            HoldDescription = holdDescription?.Trim(),
            HoldReference = holdReference?.Trim(),
            HoldInitiatedBy = initiatedBy.Trim(),
            HoldInitiatedAt = DateTime.UtcNow,
        };

        hold.RaiseDomainEvent(new LotPlacedOnHoldEvent(
            hold.HoldID, hold.LotNumber, holdReason.ToString(), holdReference));

        return hold;
    }

    public void Release(
        HoldDispositionCode dispositionCode,
        string releasedBy,
        string eSignatureRef,
        string? dispositionNotes = null)
    {
        if (HoldStatus != LotHoldStatus.Active)
            throw new DomainException("Only active holds can be released.");
        if (string.IsNullOrWhiteSpace(releasedBy)) throw new DomainException("Released by is required.");
        if (string.IsNullOrWhiteSpace(eSignatureRef)) throw new DomainException("E-signature reference is required.");

        HoldStatus = LotHoldStatus.Released;
        DispositionCode = dispositionCode;
        DispositionNotes = dispositionNotes?.Trim();
        ReleasedBy = releasedBy.Trim();
        ReleasedAt = DateTime.UtcNow;
        ESignatureRef = eSignatureRef.Trim();

        RaiseDomainEvent(new LotHoldReleasedEvent(HoldID, LotNumber, dispositionCode.ToString(), releasedBy));
    }

    public void Reject(
        HoldDispositionCode dispositionCode,
        string releasedBy,
        string eSignatureRef,
        string dispositionNotes)
    {
        if (HoldStatus != LotHoldStatus.Active)
            throw new DomainException("Only active holds can be rejected/disposed.");
        if (string.IsNullOrWhiteSpace(releasedBy)) throw new DomainException("Released by is required.");
        if (string.IsNullOrWhiteSpace(eSignatureRef)) throw new DomainException("E-signature reference is required.");
        if (string.IsNullOrWhiteSpace(dispositionNotes)) throw new DomainException("Disposition notes are required for rejection.");

        HoldStatus = dispositionCode == HoldDispositionCode.Destroy
            ? LotHoldStatus.Scrapped
            : LotHoldStatus.Rejected;
        DispositionCode = dispositionCode;
        DispositionNotes = dispositionNotes.Trim();
        ReleasedBy = releasedBy.Trim();
        ReleasedAt = DateTime.UtcNow;
        ESignatureRef = eSignatureRef.Trim();

        RaiseDomainEvent(new LotHoldRejectedEvent(HoldID, LotNumber, dispositionCode.ToString()));
    }
}
