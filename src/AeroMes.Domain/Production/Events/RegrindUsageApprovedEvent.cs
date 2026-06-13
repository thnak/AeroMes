using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record RegrindUsageApprovedEvent(
    long BlendLogID,
    long JobID,
    string ApprovedBy) : IDomainEvent;
