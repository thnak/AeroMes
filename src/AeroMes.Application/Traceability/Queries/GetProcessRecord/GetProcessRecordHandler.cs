using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetProcessRecord;

public sealed class GetProcessRecordHandler(IProcessRecordRepository repository)
    : IQueryHandler<GetProcessRecordQuery, ProcessRecordDetailDto?>
{
    public Task<ProcessRecordDetailDto?> HandleAsync(
        GetProcessRecordQuery query, CancellationToken ct)
        => repository.GetDetailAsync(query.ProcessRecordID, ct);
}
