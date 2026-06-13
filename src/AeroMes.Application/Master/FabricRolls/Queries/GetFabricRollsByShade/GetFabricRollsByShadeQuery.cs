using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollsByShade;

public record GetFabricRollsByShadeQuery(
    string FabricProductCode,
    string? ShadeCode = null) : IQuery<IReadOnlyList<FabricRollDto>>;
