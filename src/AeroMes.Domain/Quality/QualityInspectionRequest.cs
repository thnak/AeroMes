using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum InspectionRequestPurpose { PostProduction, PostOutsourcing }
public enum InspectionRequestStatus { NotStarted, InProgress, Completed }

public class QualityInspectionRequest : AuditableEntity
{
    public int RequestID { get; private set; }
    public string RequestNumber { get; private set; } = string.Empty;
    public DateOnly RequestDate { get; private set; }
    public InspectionRequestPurpose InspectionPurpose { get; private set; }
    public string RequesterName { get; private set; } = string.Empty;
    public string RequestingDepartment { get; private set; } = string.Empty;
    public string RecipientPerson { get; private set; } = string.Empty;
    public DateTime InspectionDeadline { get; private set; }
    public InspectionRequestStatus Status { get; private set; } = InspectionRequestStatus.NotStarted;

    private QualityInspectionRequest() { }

    public static QualityInspectionRequest Create(
        string requestNumber,
        DateOnly requestDate,
        InspectionRequestPurpose purpose,
        string requesterName,
        string requestingDepartment,
        string recipientPerson,
        DateTime inspectionDeadline,
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
            InspectionDeadline = inspectionDeadline,
            CreatedBy = createdBy
        };
    }

    public void Update(
        DateOnly requestDate,
        InspectionRequestPurpose purpose,
        string requesterName,
        string requestingDepartment,
        string recipientPerson,
        DateTime inspectionDeadline,
        string? updatedBy)
    {
        RequestDate = requestDate;
        InspectionPurpose = purpose;
        RequesterName = requesterName;
        RequestingDepartment = requestingDepartment;
        RecipientPerson = recipientPerson;
        InspectionDeadline = inspectionDeadline;
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
