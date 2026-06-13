using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Events;

namespace AeroMes.Domain.Production;

public class MaterialBlendLog : Entity
{
    public long BlendLogID { get; private set; }
    public long JobID { get; private set; }
    public string ResinProductCode { get; private set; } = string.Empty;
    public string VirginLotNumber { get; private set; } = string.Empty;
    public decimal VirginQtyKg { get; private set; }
    public string? RegrindLotNumber { get; private set; }
    public decimal RegrindQtyKg { get; private set; }
    public decimal MaxAllowedPct { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovalNotes { get; private set; }
    public DateTime RecordedAt { get; private set; }

    // Computed properties (not mapped to DB computed columns to stay EF-compatible)
    public decimal TotalQtyKg => VirginQtyKg + RegrindQtyKg;
    public decimal RegrindRatioPct => TotalQtyKg > 0 ? (RegrindQtyKg / TotalQtyKg) * 100m : 0m;
    public bool IsCompliant => RegrindRatioPct <= MaxAllowedPct;

    private MaterialBlendLog() { }

    public static MaterialBlendLog Record(
        long jobId,
        string resinProductCode,
        string virginLotNumber,
        decimal virginQtyKg,
        string? regrindLotNumber,
        decimal regrindQtyKg,
        decimal maxAllowedPct)
    {
        if (virginQtyKg <= 0) throw new DomainException("Virgin quantity must be positive.");
        if (regrindQtyKg < 0) throw new DomainException("Regrind quantity cannot be negative.");
        if (regrindQtyKg > 0 && string.IsNullOrWhiteSpace(regrindLotNumber))
            throw new DomainException("Regrind lot number is required when regrind quantity is greater than zero.");

        var log = new MaterialBlendLog
        {
            JobID = jobId,
            ResinProductCode = resinProductCode.Trim().ToUpperInvariant(),
            VirginLotNumber = virginLotNumber.Trim().ToUpperInvariant(),
            VirginQtyKg = virginQtyKg,
            RegrindLotNumber = regrindLotNumber?.Trim().ToUpperInvariant(),
            RegrindQtyKg = regrindQtyKg,
            MaxAllowedPct = maxAllowedPct,
            RecordedAt = DateTime.UtcNow,
        };

        if (!log.IsCompliant)
        {
            log.RaiseDomainEvent(new NonCompliantBlendRatioEvent(
                log.BlendLogID, log.JobID, log.ResinProductCode,
                log.RegrindRatioPct, log.MaxAllowedPct));
        }

        return log;
    }

    public void Approve(string approvedBy, string? approvalNotes)
    {
        if (IsCompliant)
            throw new DomainException("Cannot approve a compliant blend — approval is only required for non-compliant blends.");
        if (!string.IsNullOrWhiteSpace(ApprovedBy))
            throw new DomainException($"Blend log {BlendLogID} has already been approved by {ApprovedBy}.");

        ApprovedBy = approvedBy.Trim();
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = approvalNotes?.Trim();

        RaiseDomainEvent(new RegrindUsageApprovedEvent(BlendLogID, JobID, approvedBy));
    }
}
