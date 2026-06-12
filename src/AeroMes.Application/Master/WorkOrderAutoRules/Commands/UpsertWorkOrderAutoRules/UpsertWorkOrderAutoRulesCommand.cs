using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.UpsertWorkOrderAutoRules;

public record UpsertWorkOrderAutoRulesCommand(
    int? WorkCenterId,
    bool AutoStartEnabled,
    bool AutoCompleteOnTargetReached,
    bool RequireDeleteConfirmToken,
    int MaxConcurrentJobs,
    bool RequireCertification,
    string? UpdatedBy) : ICommand<int>;
