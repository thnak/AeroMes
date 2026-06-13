using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetLotAsBuiltRecord;

public sealed record GetLotAsBuiltRecordQuery(string LotNumber)
    : IQuery<IReadOnlyList<ProcessRecordDto>>;
