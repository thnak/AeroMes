using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetLotAsBuiltRecord;

public sealed class GetLotAsBuiltRecordHandler(IProcessRecordRepository repository)
    : IQueryHandler<GetLotAsBuiltRecordQuery, IReadOnlyList<ProcessRecordDto>>
{
    public Task<IReadOnlyList<ProcessRecordDto>> HandleAsync(
        GetLotAsBuiltRecordQuery query, CancellationToken ct)
        => repository.GetByLotNumberAsync(query.LotNumber.ToUpperInvariant(), ct);
}
