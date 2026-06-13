using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetRecallAuditLog;

public sealed class GetRecallAuditLogHandler(IRecallRepository repository)
    : IQueryHandler<GetRecallAuditLogQuery, IReadOnlyList<RecallAuditEntryDto>>
{
    public Task<IReadOnlyList<RecallAuditEntryDto>> HandleAsync(
        GetRecallAuditLogQuery query, CancellationToken ct)
        => repository.GetAuditLogAsync(query.RecallID, ct);
}
