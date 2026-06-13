using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetParametersForStep;

public sealed record GetParametersForStepQuery(Guid ProcessRecordID)
    : IQuery<IReadOnlyList<ProcessParameterDto>>;
