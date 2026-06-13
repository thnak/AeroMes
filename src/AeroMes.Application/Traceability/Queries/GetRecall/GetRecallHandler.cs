using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetRecall;

public sealed class GetRecallHandler(IRecallRepository repository)
    : IQueryHandler<GetRecallQuery, RecallDetailDto?>
{
    public Task<RecallDetailDto?> HandleAsync(GetRecallQuery query, CancellationToken ct)
        => repository.GetDetailAsync(query.RecallID, ct);
}
