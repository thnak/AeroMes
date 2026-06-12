using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolUsage;

public record RecordToolUsageCommand(
    string ToolCode,
    int Count,
    string? UpdatedBy) : ICommand<RecordToolUsageResult>;

public record RecordToolUsageResult(
    int CurrentUsageCount,
    int? MaxUsageCount,
    bool ReconditioningDue,
    bool NearingEndOfLife);
