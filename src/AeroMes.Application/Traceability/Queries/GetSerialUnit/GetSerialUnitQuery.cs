using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialUnit;

public record GetSerialUnitQuery(string SerialNumber) : IQuery<SerialUnitDetailDto?>;
