using AeroMes.Application.Master.EngChanges.Queries.GetEngChanges;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Queries.GetEngChangeByNumber;

public record GetEngChangeByNumberQuery(string EcNumber) : IQuery<EngChangeDetailDto?>;

public record EngChangeDetailDto(
    EngChangeDto Summary,
    string? Description,
    string? NewBomProductCode,
    string? NewBomVersion);
