using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialComponentLots;

public record GetSerialComponentLotsQuery(string SerialNumber) : IQuery<IReadOnlyList<SerialLotLineageDto>>;
