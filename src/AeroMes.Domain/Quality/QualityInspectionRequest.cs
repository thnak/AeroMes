using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum InspectionRequestPurpose { PostProduction, PostOutsourcing }
public enum InspectionRequestStatus { NotStarted, InProgress, Completed }
public enum InspectionPriority { Low, Medium, High }

public class QualityInspectionRequest : AuditableEntity
{
    public int RequestID { get; private set; }
    public string RequestNumber { get; private set; } = string.Empty;
    public DateOnly RequestDate { get; private set; }
    public InspectionRequestPurpose InspectionPurpose { get; private set; }
    public string RequesterName { get; private set; } = string.Empty;
    public string RequestingDepartment { get; private set; } = string.Empty;
    public string RecipientPerson { get; private set; } = string.Empty;
    public string? RecipientDepartment { get; private set; }
    public DateTime InspectionDeadline { get; private set; }
    public InspectionRequestStatus Status { get; private set; } = InspectionRequestStatus.NotStarted;

    // Extended fields
    public decimal? InspectionQuantity { get; private set; }
    public InspectionPriority? Priority { get; private set; }
    public string? Description { get; private set; }

    // PostProduction-specific
    public int? ProductionOrderId { get; private set; }
    public int? StatisticalSheetId { get; private set; }
    public string? InspectionSubject { get; private set; }

    // PostOutsourcing-specific
    public int? SubcontractingOrderId { get; private set; }
    public int? ProductId { get; private set; }

    private QualityInspectionRequest() { }

    public static QualityInspectionRequest Create(
        string requestNumber,
        DateOnly requestDate,
        InspectionRequestPurpose purpose,
        string requesterName,
        string requestingDepartment,
        string recipientPerson,
        string? recipientDepartment,
        DateTime inspectionDeadline,
        decimal? inspectionQuantity,
        InspectionPriority? priority,
        string? description,
        int? productionOrderId,
        int? statisticalSheetId,
        string? inspectionSubject,
        int? subcontractingOrderId,
        int? productId,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(requestNumber))
            throw new DomainException("Số phiếu yêu cầu không được để trống.");
        return new QualityInspectionRequest
        {
            RequestNumber = requestNumber,
            RequestDate = requestDate,
            InspectionPurpose = purpose,
            RequesterName = requesterName,
            RequestingDepartment = requestingDepartment,
            RecipientPerson = recipientPerson,
            RecipientDepartment = recipientDepartment,
            InspectionDeadline = inspectionDeadline,
            InspectionQuantity = inspectionQuantity,
            Priority = priority,
            Description = description,
            ProductionOrderId = productionOrderId,
            StatisticalSheetId = statisticalSheetId,
            InspectionSubject = inspectionSubject,
            SubcontractingOrderId = subcontractingOrderId,
            ProductId = productId,
            CreatedBy = createdBy
        };
    }

    public void Update(
        DateOnly requestDate,
        InspectionRequestPurpose purpose,
        string requesterName,
        string requestingDepartment,
        string recipientPerson,
        string? recipientDepartment,
        DateTime inspectionDeadline,
        decimal? inspectionQuantity,
        InspectionPriority? priority,
        string? description,
        int? productionOrderId,
        int? statisticalSheetId,
        string? inspectionSubject,
        int? subcontractingOrderId,
        int? productId,
        string? updatedBy)
    {
        RequestDate = requestDate;
        InspectionPurpose = purpose;
        RequesterName = requesterName;
        RequestingDepartment = requestingDepartment;
        RecipientPerson = recipientPerson;
        RecipientDepartment = recipientDepartment;
        InspectionDeadline = inspectionDeadline;
        InspectionQuantity = inspectionQuantity;
        Priority = priority;
        Description = description;
        ProductionOrderId = productionOrderId;
        StatisticalSheetId = statisticalSheetId;
        InspectionSubject = inspectionSubject;
        SubcontractingOrderId = subcontractingOrderId;
        ProductId = productId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(InspectionRequestStatus status, string? updatedBy)
    {
        Status = status;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
