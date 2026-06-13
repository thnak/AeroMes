using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.CheckLotHoldStatus;

public sealed record CheckLotHoldStatusQuery(string LotNumber)
    : IQuery<LotHoldStatusDto>;
