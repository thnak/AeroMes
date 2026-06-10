using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.UnitOfMeasures.Queries.GetAllUoMs;

public record GetAllUoMsQuery : IQuery<IReadOnlyList<UoMDto>>;

public record UoMDto(string UoMCode, string UoMName, string UoMGroup);
