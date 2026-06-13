using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetParametersForStep;

public sealed class GetParametersForStepHandler(IProcessRecordRepository repository)
    : IQueryHandler<GetParametersForStepQuery, IReadOnlyList<ProcessParameterDto>>
{
    public Task<IReadOnlyList<ProcessParameterDto>> HandleAsync(
        GetParametersForStepQuery query, CancellationToken ct)
        => repository.GetParametersAsync(query.ProcessRecordID, ct);
}
