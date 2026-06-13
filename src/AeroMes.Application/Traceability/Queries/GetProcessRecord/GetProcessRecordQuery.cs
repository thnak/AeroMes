using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetProcessRecord;

public sealed record GetProcessRecordQuery(Guid ProcessRecordID)
    : IQuery<ProcessRecordDetailDto?>;
