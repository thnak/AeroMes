using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetRecallAuditLog;

public sealed record GetRecallAuditLogQuery(Guid RecallID)
    : IQuery<IReadOnlyList<RecallAuditEntryDto>>;
