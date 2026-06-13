using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetRecallScope;

public sealed class GetRecallScopeHandler(IRecallRepository repository)
    : IQueryHandler<GetRecallScopeQuery, RecallScopeDto>
{
    public Task<RecallScopeDto> HandleAsync(GetRecallScopeQuery query, CancellationToken ct)
        => repository.GetScopeAsync(query.RecallID, ct);
}
