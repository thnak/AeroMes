using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Queries.GetWorkOrderAutoRules;

public record GetWorkOrderAutoRulesQuery : IQuery<IReadOnlyList<WorkOrderAutoRulesDto>>;

public record WorkOrderAutoRulesDto(
    int RuleId,
    int? WorkCenterId,
    bool AutoStartEnabled,
    bool AutoCompleteOnTargetReached,
    bool RequireDeleteConfirmToken,
    int MaxConcurrentJobs,
    bool RequireCertification);
