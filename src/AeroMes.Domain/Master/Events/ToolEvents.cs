using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master.Events;

public record ToolReconditioningDueEvent(
    int ToolId, string ToolCode, int CurrentUsageCount, int UsageCountAtLastPm, int PmIntervalCount) : IDomainEvent;

public record ToolNearingEndOfLifeEvent(
    int ToolId, string ToolCode, int CurrentUsageCount, int MaxUsageCount) : IDomainEvent;

public record ToolCalibrationDueEvent(
    int ToolId, string ToolCode, DateOnly NextCalibrationDue) : IDomainEvent;
