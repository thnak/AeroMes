using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class WorkOrderAutoRules : AuditableEntity
{
    public int RuleId { get; private set; }
    public int? WorkCenterId { get; private set; }
    public bool AutoStartEnabled { get; private set; }
    public bool AutoCompleteOnTargetReached { get; private set; }
    public bool RequireDeleteConfirmToken { get; private set; }
    public bool RequireCertification { get; private set; }
    public int MaxConcurrentJobs { get; private set; } = 1;

    public WorkCenter? WorkCenter { get; private set; }

    private WorkOrderAutoRules() { }

    public static WorkOrderAutoRules Create(
        int? workCenterId,
        bool autoStartEnabled,
        bool autoCompleteOnTargetReached,
        bool requireDeleteConfirmToken,
        int maxConcurrentJobs = 1,
        bool requireCertification = false,
        string? createdBy = null)
    {
        return new WorkOrderAutoRules
        {
            WorkCenterId = workCenterId,
            AutoStartEnabled = autoStartEnabled,
            AutoCompleteOnTargetReached = autoCompleteOnTargetReached,
            RequireDeleteConfirmToken = requireDeleteConfirmToken,
            RequireCertification = requireCertification,
            MaxConcurrentJobs = maxConcurrentJobs,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        bool autoStartEnabled, bool autoCompleteOnTargetReached,
        bool requireDeleteConfirmToken, int maxConcurrentJobs,
        bool requireCertification, string? updatedBy)
    {
        AutoStartEnabled = autoStartEnabled;
        AutoCompleteOnTargetReached = autoCompleteOnTargetReached;
        RequireDeleteConfirmToken = requireDeleteConfirmToken;
        RequireCertification = requireCertification;
        MaxConcurrentJobs = maxConcurrentJobs;
        Touch(updatedBy);
    }
}
