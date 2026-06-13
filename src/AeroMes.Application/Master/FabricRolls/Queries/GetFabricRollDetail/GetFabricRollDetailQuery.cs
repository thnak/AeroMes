using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollDetail;

public record GetFabricRollDetailQuery(int RollID) : IQuery<FabricRollDto?>;
