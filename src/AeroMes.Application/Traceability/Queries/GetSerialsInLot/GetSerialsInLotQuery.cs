using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialsInLot;

public record GetSerialsInLotQuery(
    string LotNumber,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<SerialUnitDto>>;
