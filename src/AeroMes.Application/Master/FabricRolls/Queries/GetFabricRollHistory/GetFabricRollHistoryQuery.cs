using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollHistory;

public record GetFabricRollHistoryQuery(int RollID) : IQuery<IReadOnlyList<FabricConsumptionLogDto>>;
