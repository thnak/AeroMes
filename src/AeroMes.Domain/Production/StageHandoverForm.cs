using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum HandoverFormType { Handover, Return }
public enum HandoverFormStatus { Draft, Submitted, Confirmed, Cancelled }

public class StageHandoverForm : AuditableEntity
{
    public int FormID { get; private set; }
    public string FormNumber { get; private set; } = string.Empty;
    public HandoverFormType FormType { get; private set; }
    public HandoverFormStatus Status { get; private set; } = HandoverFormStatus.Draft;

    // Source
    public int FromWorkOrderID { get; private set; }
    public int FromRoutingStepID { get; private set; }

    // Destination (may be a different work order — cross-order)
    public int ToWorkOrderID { get; private set; }
    public int ToRoutingStepID { get; private set; }

    public DateTime HandoverDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<HandoverLineItem> _lines = [];
    public IReadOnlyList<HandoverLineItem> Lines => _lines.AsReadOnly();

    private StageHandoverForm() { }

    public static StageHandoverForm Create(
        string formNumber,
        HandoverFormType formType,
        int fromWorkOrderId,
        int fromRoutingStepId,
        int toWorkOrderId,
        int toRoutingStepId,
        DateTime handoverDate,
        string? notes,
        string? createdBy)
    {
        if (fromRoutingStepId == toRoutingStepId && fromWorkOrderId == toWorkOrderId)
            throw new DomainException("Source and destination stage cannot be the same.");

        return new StageHandoverForm
        {
            FormNumber = formNumber.Trim(),
            FormType = formType,
            FromWorkOrderID = fromWorkOrderId,
            FromRoutingStepID = fromRoutingStepId,
            ToWorkOrderID = toWorkOrderId,
            ToRoutingStepID = toRoutingStepId,
            HandoverDate = handoverDate,
            Notes = notes?.Trim(),
            Status = HandoverFormStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void AddLine(string productCode, decimal qty, string unit, string? notes)
    {
        if (Status != HandoverFormStatus.Draft)
            throw new DomainException("Cannot modify a submitted or confirmed form.");
        if (qty <= 0)
            throw new DomainException("Handover quantity must be positive.");

        _lines.Add(HandoverLineItem.Create(FormID, productCode, qty, unit, notes));
    }

    public void Submit(string updatedBy)
    {
        if (Status != HandoverFormStatus.Draft)
            throw new DomainException($"Form must be Draft to submit. Current: {Status}.");
        if (_lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        Status = HandoverFormStatus.Submitted;
        Touch(updatedBy);
    }

    public void Confirm(string updatedBy)
    {
        if (Status != HandoverFormStatus.Submitted)
            throw new DomainException($"Form must be Submitted to confirm. Current: {Status}.");

        Status = HandoverFormStatus.Confirmed;
        Touch(updatedBy);
    }

    public void Cancel(string updatedBy)
    {
        if (Status == HandoverFormStatus.Confirmed)
            throw new DomainException("Cannot cancel a confirmed form.");

        Status = HandoverFormStatus.Cancelled;
        Touch(updatedBy);
    }
}

public class HandoverLineItem
{
    public int LineID { get; private set; }
    public int FormID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public decimal Qty { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    private HandoverLineItem() { }

    internal static HandoverLineItem Create(int formId, string productCode, decimal qty, string unit, string? notes)
        => new()
        {
            FormID = formId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            Qty = qty,
            Unit = unit.Trim(),
            Notes = notes?.Trim(),
        };
}
