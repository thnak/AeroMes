using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetAffectedSerialsFromLot;

public record GetAffectedSerialsFromLotQuery(string ComponentLotNumber)
    : IQuery<IReadOnlyList<SerialUnitDto>>;
