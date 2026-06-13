using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Events;

namespace AeroMes.Domain.Quality;

public class InspectionOrder : Entity
{
    public int InspectionOrderId { get; private set; }
    public string OrderNo { get; private set; } = string.Empty;
    public int PlanId { get; private set; }
    public long JobId { get; private set; }
    public long WorkOrderId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string? LotNumber { get; private set; }
    public int SampleSize { get; private set; }
    public string TriggeredBy { get; private set; } = string.Empty;  // AUTO_ON_STEP_COMPLETE | MANUAL
    public string Status { get; private set; } = string.Empty;       // PENDING | ASSIGNED | IN_PROGRESS | PASSED | FAILED | WAIVED
    public string? InspectorCode { get; private set; }
    public DateTimeOffset? AssignedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? WaivedBy { get; private set; }
    public string? WaivedReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Nav props
    public InspectionPlan? Plan { get; private set; }

    private InspectionOrder() { }

    public static InspectionOrder Create(
        string orderNo,
        int planId,
        long jobId,
        long workOrderId,
        string productCode,
        string? lotNumber,
        int sampleSize,
        string triggeredBy)
    {
        return new InspectionOrder
        {
            OrderNo = orderNo,
            PlanId = planId,
            JobId = jobId,
            WorkOrderId = workOrderId,
            ProductCode = productCode,
            LotNumber = lotNumber,
            SampleSize = sampleSize,
            TriggeredBy = triggeredBy,
            Status = "PENDING",
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void Assign(string inspectorCode)
    {
        if (Status != "PENDING")
            throw new DomainException($"Cannot assign order in status {Status}.");
        Status = "ASSIGNED";
        InspectorCode = inspectorCode;
        AssignedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        if (Status is not ("PENDING" or "ASSIGNED"))
            throw new DomainException($"Cannot start order in status {Status}.");
        Status = "IN_PROGRESS";
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Pass()
    {
        if (Status != "IN_PROGRESS")
            throw new DomainException($"Cannot pass order in status {Status}.");
        Status = "PASSED";
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        if (Status != "IN_PROGRESS")
            throw new DomainException($"Cannot fail order in status {Status}.");
        Status = "FAILED";
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new InspectionOrderFailedEvent(InspectionOrderId, WorkOrderId, ProductCode));
    }

    public void Waive(string waivedBy, string reason)
    {
        if (Status is "PASSED" or "FAILED" or "WAIVED")
            throw new DomainException($"Cannot waive order in status {Status}.");
        Status = "WAIVED";
        WaivedBy = waivedBy;
        WaivedReason = reason;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
